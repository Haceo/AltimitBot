using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using System.Collections.Concurrent;
using Discord.Audio;
using System.Diagnostics;
using System.IO;
using System.Threading;

namespace AltimitBot2._0.Modules
{
    public class Audio : ModuleBase<SocketCommandContext>
    {
        IAudioClient audioClient;
        private static readonly string DLPath = Path.Combine(Directory.GetCurrentDirectory(), "Temp");
        public ConcurrentQueue<Tracks> currentList = new ConcurrentQueue<Tracks>();
        /*[Command("join", RunMode = RunMode.Async)]
        [RequireUserPermission(ChannelPermission.ManageChannels)]
        public async Task join(IVoiceChannel channel = null)
        {
            channel = channel ?? (Context.User as IGuildUser)?.VoiceChannel;
            if (channel == null)
            {
                await Misc.EmbedWriter(Context.Channel, Context.User,
                    "Error!",
                    "User must be in a voice channel or a voice channel must be passed as an argument.");
                return;
            }
            audioClient = await channel.ConnectAsync();
        }*/
        [Command("add", RunMode = RunMode.Async)]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task add(string url)
        {
            string file = "";
            if (url.ToLower().Contains("youtube.com"))
            {
                try
                {
                    CommandHandler.consoleOut("GET " + url);
                    file = await YoutubeDL(url);
                    Tuple<string, string> info = await YoutubeInfo(url);
                    string title;
                    string duration;
                    info.Deconstruct(out title, out duration);
                    Tracks newTrack = new Tracks();
                    newTrack.Title = title;
                    newTrack.Durration = duration;
                    newTrack.Path = file;
                    newTrack.Server = Context.Guild.Id;
                    BotConfig.playList.Add(newTrack);
                    currentList.Enqueue(newTrack);
                    BotConfig.SavePlaylist();
                    CommandHandler.consoleOut("DONE");
                }
                catch (Exception ex)
                {
                    CommandHandler.consoleOut(ex.ToString());
                }
            }
        }
        [Command("play", RunMode = RunMode.Async)]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task play(IVoiceChannel channel = null)
        {
            channel = channel ?? (Context.User as IGuildUser)?.VoiceChannel;
            if (channel == null)
            {
                await Misc.EmbedWriter(Context.Channel, Context.User,
                    "Error!",
                    "User must be in a voice channel or a voice channel must be passed as an argument.");
                return;
            }
            audioClient = await channel.ConnectAsync();
            foreach (var track in BotConfig.playList.Where(x => x.Server == Context.Guild.Id))
            {
                currentList.Enqueue(track);
            }
            try
            {
                while (currentList.TryDequeue(out var track))
                {
                    CommandHandler.consoleOut("Playing" + track.Title);
                    await SendAsync(track.Path);
                    File.Delete(track.Path);
                    BotConfig.playList.Remove(BotConfig.playList.FirstOrDefault(x => x.Path == track.Path && x.Server == Context.Guild.Id));
                    BotConfig.SavePlaylist();
                    await Task.Delay(4000);
                }
            }
            catch (Exception ex)
            {
                CommandHandler.consoleOut(ex.ToString());
            }
            await channel.DisconnectAsync();
        }
        private async Task SendAsync(string path)
        {
            using (Process ffmpeg = CreateStream(path))
            using (Stream output = ffmpeg.StandardOutput.BaseStream)
            using (AudioOutStream discord = audioClient.CreatePCMStream(AudioApplication.Music))
            {
                await output.CopyToAsync(discord);
                await discord.FlushAsync();
            }
        }
        private Process CreateStream(string path)
        {
            return Process.Start(new ProcessStartInfo
            {
                FileName = "ffmpeg",
                Arguments = $"-loglevel panic -i \"{path}\" -ac 2 -f s16le -ar 48000 pipe:1",
                UseShellExecute = false,
                RedirectStandardOutput = true,
            });
        }
        private async Task<string> YoutubeDL(string url)
        {
            TaskCompletionSource<string> tcs = new TaskCompletionSource<string>();
            new Thread(() =>
            {
                string file;
                int count = 0;
                do
                {
                    file = Path.Combine(DLPath, "botsong" + ++count + ".mp3");
                }
                while (File.Exists(file));
                Process youtubedl;

                ProcessStartInfo youtubeDownload = new ProcessStartInfo()
                {
                    FileName = "youtube-dl",
                    Arguments = $"-x --audio-format mp3 -o \"{file.Replace(".mp3", ".%(ext)s")}\" {url}",
                    CreateNoWindow = true,
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                };
                youtubedl = Process.Start(youtubeDownload);
                youtubedl.WaitForExit();
                Thread.Sleep(1000);
                if (File.Exists(file))
                {
                    tcs.SetResult(file);
                }
                else
                {
                    tcs.SetResult(null);
                }
            }).Start();

            string result = await tcs.Task;
            if (result == null)
                throw new Exception("youtube-dl.exe failed to download!");

            result = result.Replace("\n", "").Replace(Environment.NewLine, "");

            return result;
        }
        private static async Task<Tuple<string, string>> YoutubeInfo(string url)
        {
            TaskCompletionSource<Tuple<string, string>> tcs = new TaskCompletionSource<Tuple<string, string>>();

            new Thread(() =>
            {
                string title;
                string duration;

                Process youtubedl;

                ProcessStartInfo youtubedlGetTitle = new ProcessStartInfo()
                {
                    FileName = "youtube-dl",
                    Arguments = $"-s -e --get-duration {url}",
                    CreateNoWindow = true,
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                };
                youtubedl = Process.Start(youtubedlGetTitle);
                youtubedl.WaitForExit();
                string[] lines = youtubedl.StandardOutput.ReadToEnd().Split('\n');

                if (lines.Length >= 2)
                {
                    title = lines[0];
                    duration = lines[1];
                }
                else
                {
                    title = "No title found";
                    duration = "0";
                }

                tcs.SetResult(new Tuple<string, string>(title, duration));
            }).Start();

            Tuple<string, string> result = await tcs.Task;
            if (result == null)
                throw new Exception("youtube-dl.exe failed to receive title!");
            return result;
        }
    }
}
