using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;
using Discord;
using Discord.Audio;
using Discord.Commands;

namespace Altimit_v3.Modules
{
    public class Audio : ModuleBase<SocketCommandContext>
    {
        private static readonly string tempDir = Path.Combine(Directory.GetCurrentDirectory(), "Temp");
        IAudioClient _audioClient;
        public static MainWindow _main;
        bool playing = false;
        //-----Commands----------------------------------------------------------------------------------------------------
        [Command("add", RunMode = RunMode.Async)]
        public async Task AddTrack(string url)
        {
            await Task.Delay(200);
            await Context.Channel.DeleteMessageAsync(Context.Message);
            var server = _main.ServerList.FirstOrDefault(x => x.ServerId == Context.Guild.Id);
            if (url.ToLower().Contains("youtube.com"))
            {
                try
                {
                    BotFrame.consoleOut($"GET {url}");
                    Tuple<string, string> info = await YoutubeInfo(url);
                    string[] time = info.Item2.Split(':');
                    int min = 0;
                    int sec = 0;
                    if (server.MaxLength != 0)
                    {
                        if (time.Length == 3)
                        {
                            min = (int.Parse(time[0]) * 60) + int.Parse(time[1]);
                            sec = int.Parse(time[2]);
                        }
                        else if (time.Length == 2)
                        {
                            min = int.Parse(time[0]);
                            sec = int.Parse(time[1]);
                        }
                        if (sec > 30)
                            min++;
                        if (server.MaxLength < min)
                        {
                            await BotFrame.EmbedWriter(Context.Channel, Context.User,
                                "Altimit Audio",
                                $"Song must be under {server.MaxLength} minutes long!" + Environment.NewLine +
                                $"Length: {info.Item2}");
                        }
                    }
                    string file = await YoutubeDL(url);
                    if (server.SongList == null)
                        server.SongList = new List<Song>();
                    server.SongList.Add(new Song()
                    {
                        Title = info.Item1,
                        Duration = info.Item2,
                        User = Context.User.Username,
                        Path = file
                    });
                    BotFrame.SaveFile("servers");
                    BotFrame.consoleOut("DONE");
                }
                catch (Exception ex)
                {
                    BotFrame.consoleOut($"ERROR {ex.Message}");
                }
            }
        }
        [Command("remove", RunMode = RunMode.Async)]
        public async Task RemoveTrack(int trackNumber)
        {
            await Task.Delay(200);
            await Context.Channel.DeleteMessageAsync(Context.Message);
            var server = _main.ServerList.FirstOrDefault(x => x.ServerId == Context.Guild.Id);
            if (trackNumber <= 0 || trackNumber > server.SongList.Count)
            {
                await BotFrame.EmbedWriter(Context.Channel, Context.User,
                    "Altimit",
                    "Please select a track that exists in the list.");
                return;
            }
            File.Delete(server.SongList[trackNumber--].Path);
            server.SongList.Remove(server.SongList[trackNumber--]);
            BotFrame.SaveFile("servers");
        }
        [Command("playlist", RunMode = RunMode.Async)]
        public async Task ListSongs()
        {
            await Task.Delay(200);
            await Context.Channel.DeleteMessageAsync(Context.Message);
            var server = _main.ServerList.FirstOrDefault(x => x.ServerId == Context.Guild.Id);
            string songList = "";
            int i = 0;
            foreach (var song in server.SongList)
            {
                i++;
                songList = songList + $"{i}. {song.Title} - {song.Duration} - Added by: {song.User}" + Environment.NewLine;
            }
            await BotFrame.EmbedWriter(Context.Channel, Context.User,
                "Altimit",
                $"Playlist:" + Environment.NewLine + songList, time: 60000);
        }
        [Command("play", RunMode = RunMode.Async)]
        public async Task PlaySongs(IVoiceChannel channel = null)
        {
            await Task.Delay(200);
            await Context.Channel.DeleteMessageAsync(Context.Message);
            var server = _main.ServerList.FirstOrDefault(x => x.ServerId == Context.Guild.Id);
            if (server.BotChannel == 0)
            channel = channel ?? (Context.User as IGuildUser)?.VoiceChannel;
            if (channel == null)
            {
                await BotFrame.EmbedWriter(Context.Channel, Context.User,
                    "Altimit Audio",
                    "User must be in a voice channel or a voice channel must be passed as an argument.");
                return;
            }
            if (playing)
            {
                await BotFrame.EmbedWriter(Context.Channel, Context.User,
                    "Altimit Audio",
                    "Player already in another channel please wait for the bot to become available or utilize the channel already in use.");
            }
            _audioClient = await channel.ConnectAsync();
            playing = true;
            PlaybackLoop(server);
        }
        [Command("stop", RunMode = RunMode.Async)]
        public async Task StopPlaying()
        {
            await Task.Delay(200);
            await Context.Channel.DeleteMessageAsync(Context.Message);
            playing = false;
            await _audioClient.StopAsync();
            _audioClient.Dispose();
        }
        //-----playback loop----------------------------------------------------------------------------------------------------------------------------------------
        private async Task PlaybackLoop(DiscordServer server)
        {
            while (server.SongList.Count != 0 && playing)
            {
                var track = server.SongList.First();
                BotFrame.consoleOut($"Playing: {track.Title} - Added by: {track.User}");
                try
                {
                    await SendAsync(track.Path);
                }
                catch (Exception ex)
                {
                    BotFrame.consoleOut($"Playback Error! {ex.Message}");
                    return;
                }
                if (!server.LoopOne)
                {
                    File.Delete(track.Path);
                    server.SongList.Remove(track);
                    BotFrame.SaveFile("servers");
                }
                await Task.Delay(4000);
                if (!server.Continuous)
                    return;
            }
        }
        //-----ffmpeg------------------------------------------------------------------------------------------------------------------------------------------------
        public async Task SendAsync(string path)
        {
            using (Process ffmpeg = CreateStream(path))
            using (Stream output = ffmpeg.StandardOutput.BaseStream)
            using (AudioStream discord = _audioClient.CreatePCMStream(AudioApplication.Music))
            {
                await output.CopyToAsync(discord);
                await discord.FlushAsync();
            }
        }
        private static Process CreateStream(string path)
        {
            return Process.Start(new ProcessStartInfo
            {
                FileName = "ffmpeg",
                Arguments = $"-loglevel panic -i \"{path}\" -ac 2 -f s16le -ar 48000 pipe:1",
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
            });
        }
        //-----YT-DL------------------------------------------------------------------------------------------------------------------------------------------------
        public async Task<string> YoutubeDL(string url)
        {
            TaskCompletionSource<string> tcs = new TaskCompletionSource<string>();
            new Thread(() =>
            {
                string file;
                int count = 0;
                do
                    file = Path.Combine(tempDir, $"botsong{++count}.mp3");
                while (File.Exists(file));
                Process.Start(new ProcessStartInfo
                {
                    FileName = "youtube-dl",
                    Arguments = $"-x --audio-format mp3 -o \"{file.Replace(".mp3", ".%(ext)s")}\" {url}",
                    CreateNoWindow = true,
                    UseShellExecute = false
                }).WaitForExit();
                Thread.Sleep(1000);
                if (File.Exists(file))
                    tcs.SetResult(file);
                else
                    tcs.SetResult(null);
            }).Start();
            string res = await tcs.Task;
            if (res == null)
                throw new Exception("youtube-dl.exe failed to download!");
            res = res.Replace("\n", "").Replace(Environment.NewLine, "");
            return res;
        }
        //-----YT-Info------------------------------------------------------------------------------------------------------------------------------------------------
        public async Task<Tuple<string, string>> YoutubeInfo(string url)
        {
            TaskCompletionSource<Tuple<string, string>> tcs = new TaskCompletionSource<Tuple<string, string>>();
            new Thread(() =>
            {
                string title;
                string duration;
                Process youtubedl;
                ProcessStartInfo youtubeGetTitle = new ProcessStartInfo()
                {
                    FileName = "youtube-dl.exe",
                    Arguments = $"-s -e --get-duration {url}",
                    CreateNoWindow = true,
                    RedirectStandardOutput = true,
                    UseShellExecute = false
                };
                youtubedl = Process.Start(youtubeGetTitle);
                youtubedl.WaitForExit();
                string[] lines = youtubedl.StandardOutput.ReadToEnd().Split('\n');
                if (lines.Length >= 2)
                {
                    title = lines[0];
                    duration = lines[1];
                }
                else
                {
                    title = "No title found!";
                    duration = "0";
                }
                tcs.SetResult(new Tuple<string, string>(title, duration));
            }).Start();
            Tuple<string, string> res = await tcs.Task;
            if (res == null)
                throw new Exception("youtube-dl.exe failed to receive title!");
            return res;
        }
    }
}
