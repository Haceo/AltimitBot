using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
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
        [Command("play", RunMode = RunMode.Async)]
        [RequireUserPermission(ChannelPermission.ManageChannels)]
        public async Task play(string url, IVoiceChannel channel = null)
        {
            channel = channel ?? (Context.User as IGuildUser)?.VoiceChannel;
            if (channel == null)
            {
                await Misc.EmbedWriter(Context.Channel, Context.User,
                    "Error!",
                    "User must be in a voice channel or a voice channel must be passed as an argument.");
                return;
            }
            channel = channel ?? (Context.User as IGuildUser)?.VoiceChannel;
            audioClient = await channel.ConnectAsync();
            string file = "";
            Tuple<string, string> info = null;
            if (url.ToLower().Contains("youtube.com"))
            {
                file = await YoutubeDL(url);
                //info = await YoutubeInfo(url);
            }
            /*string title = "";
            string duration = "";
            info.Deconstruct(out title, out duration);*/
            await SendAsync(file);
        }
        private async Task SendAsync(string path)
        {
            using (Process ffmpeg = CreateStream(path))
            using (Stream output = ffmpeg.StandardOutput.BaseStream)
            using (AudioOutStream discord = audioClient.CreatePCMStream(AudioApplication.Music))
            {
                await output.CopyToAsync(discord);
                await discord.FlushAsync();
                File.Delete(path);
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
