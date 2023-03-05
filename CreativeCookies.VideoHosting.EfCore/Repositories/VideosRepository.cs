using CreativeCookies.VideoHosting.Contracts.Models;
using CreativeCookies.VideoHosting.Contracts.Repositories;
using CreativeCookies.VideoHosting.EfCore.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CreativeCookies.VideoHosting.EfCore.Repositories
{
    public class VideosRepository : IVideosRepository
    {
        private readonly VideoHostingDbContext _context;

        public VideosRepository(VideoHostingDbContext context)
        {
            _context = context;
        }

        public async Task<bool> IsPresentInDatabase(Guid id, CancellationToken token)
        {
            return await _context.Videos.Where(x => x.Id == id).FirstOrDefaultAsync(token) != null;
        }

        public async Task DeleteVideo(Guid id, CancellationToken token)
        {
            var entity = await  _context.Videos.Where(v => v.Id.Equals(id)).FirstOrDefaultAsync(token);
            if (entity != null) {
                _context.Remove(entity);
                await _context.SaveChangesAsync(token);
            }
        }

        public async Task<IEnumerable<IVideo>> GetAll(CancellationToken token)
        {
            return await _context.Videos.ToListAsync(token);
        }

        public async Task<IVideo> GetVideo(Guid id, CancellationToken token)
        {
            return await _context.Videos.Where(v => v.Id.Equals(id)).FirstOrDefaultAsync(token);
        }

        public async Task<IVideo> PostVideo(IVideo video, CancellationToken token = default)
        {
            // Open the video file as a FileStream
            using (var fileStream = new FileStream($"{video.Location}/{video.Name}", FileMode.Open))
            {
                // Split the video into segments using FFmpeg
                var segmentFiles = new List<string>();
                var segmentDuration = 1; // Duration of each segment in seconds
                var segmentNumber = 0;
                var ffmpegPath = "C:/ffmpeg/bin/ffmpeg.exe"; // Path to the FFmpeg executable
                var ffplayPath = "C:/ffmpeg/bin/ffplay.exe";
                var ffpobePath = "C:/ffmpeg/bin/ffprobe.exe";
                var outputPath = "C:/VideoParts"; // Path to the folder where segment files will be saved
                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = ffmpegPath,
                        Arguments = $"-i {video.Location}/{video.Name} -f segment -segment_time {segmentDuration} -c copy {outputPath}/segment_%d.mp4",
                        RedirectStandardInput = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    }
                };
                process.Start();
                var buffer = new byte[4096];
                int bytesRead;
                while ((bytesRead = await fileStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                {
                    await process.StandardInput.BaseStream.WriteAsync(buffer, 0, bytesRead);
                }
                process.StandardInput.Flush();
                process.StandardInput.Close();
                process.WaitForExit();
                var videoDto = new Video(video.Name, video.Location, video.Location);
                // Save video file:
                _context.Videos.Add(videoDto);
                await _context.SaveChangesAsync();
                // Read all segments from outputPath and then save them to database
                segmentFiles.AddRange(Directory.GetFiles(outputPath));


                // Save the segments to the database

                foreach (var segmentFile in segmentFiles)
                {
                    using (var segmentStream = new FileStream(segmentFile, FileMode.Open)) 
                    {
                        var segment = new VideoSegment
                        {
                            VideoId = videoDto.Id,
                            SequenceNumber = segmentNumber++,
                            Data = new byte[segmentStream.Length],
                            Video = videoDto
                        };
                        await segmentStream.ReadAsync(segment.Data, 0, segment.Data.Length);
                        _context.VideoSegments.Add(segment);
                    }
                }
                await _context.SaveChangesAsync();

                // Clear the TEMP folder
                // HACK: TODO

                return video;
            }
        }

        public async Task<IVideo> UpdateVideo(IVideo video, CancellationToken token)
        {
            var res = _context.Update(video);
            await _context.SaveChangesAsync(token);
            return res?.Entity;
        }
    }
}
