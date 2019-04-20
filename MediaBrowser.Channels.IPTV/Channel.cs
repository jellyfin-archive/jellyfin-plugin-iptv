using MediaBrowser.Controller.Channels;
using MediaBrowser.Controller.Drawing;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Model.Channels;
using MediaBrowser.Model.Drawing;
using MediaBrowser.Model.Entities;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediaBrowser.Model.Dto;

namespace MediaBrowser.Channels.IPTV
{
    public class Channel : IChannel, IHasCacheKey
    {
        private readonly ILogger _logger;

        public Channel(ILogger<Channel> logger)
        {
            _logger = logger;
        }

        // Increment as needed to invalidate all caches
        public string DataVersion => "1";

        public Task<ChannelItemResult> GetChannelItems(InternalChannelItemQuery query, CancellationToken cancellationToken)
        {
            _logger.LogDebug("cat ID : {Id}", query.FolderId);

            return GetChannelItemsInternal(cancellationToken);
        }


        private Task<ChannelItemResult> GetChannelItemsInternal(CancellationToken cancellationToken)
        {
            var items = new List<ChannelItemInfo>();

            foreach (var s in Plugin.Instance.Configuration.Bookmarks)
            {
                // Until we have user configuration in the UI, we have to disable this.
                //if (!string.Equals(s.UserId, userId, StringComparison.OrdinalIgnoreCase))
                //{
                //    continue;
                //}

                var item = new ChannelItemInfo
                {
                    Name = s.Name,
                    ImageUrl = s.Image,
                    Id = s.Name,
                    Type = ChannelItemType.Media,
                    ContentType = ChannelMediaContentType.Clip,
                    MediaType = ChannelMediaType.Video,

                    MediaSources = new List<MediaSourceInfo>
                    {
                        new ChannelMediaInfo
                        {
                            Path = s.Path,
                            Protocol = s.Protocol

                        }.ToMediaSource()
                    }
                };

                items.Add(item);
            }

            return Task.FromResult(new ChannelItemResult
            {
                Items = items
            });
        }

        public IEnumerable<ImageType> GetSupportedChannelImages()
        {
            return new List<ImageType>
            {
                ImageType.Thumb,
                ImageType.Backdrop
            };
        }

        public string HomePageUrl => string.Empty;

        public string Name => "IPTV";

        public InternalChannelFeatures GetChannelFeatures()
        {
            return new InternalChannelFeatures
            {
                ContentTypes = new List<ChannelMediaContentType>
                {
                    ChannelMediaContentType.Clip
                },

                MediaTypes = new List<ChannelMediaType>
                {
                    ChannelMediaType.Video
                },

                SupportsContentDownloading = true
            };
        }

        public Task<DynamicImageResponse> GetChannelImage(ImageType type, CancellationToken cancellationToken)
        {
            switch (type)
            {
                case ImageType.Thumb:
                case ImageType.Backdrop:
                    {
                        var path = GetType().Namespace + ".Images." + type.ToString().ToLowerInvariant() + ".png";

                        return Task.FromResult(new DynamicImageResponse
                        {
                            Format = ImageFormat.Png,
                            HasImage = true,
                            Stream = typeof(Channel).Assembly.GetManifestResourceStream(path)
                        });
                    }
                default:
                    throw new ArgumentException("Unsupported image type: " + type);
            }
        }

        public bool IsEnabledFor(string userId)
        {
            return true;
        }

        public ChannelParentalRating ParentalRating
            => ChannelParentalRating.GeneralAudience;

        public string GetCacheKey(string userId)
            => Guid.NewGuid().ToString("N");

        public string Description => string.Empty;
    }
}
