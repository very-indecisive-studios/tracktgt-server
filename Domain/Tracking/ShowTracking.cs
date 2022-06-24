﻿using Domain.Media;

namespace Domain.Tracking;

#nullable disable

public enum ShowTrackingStatus { Completed, Watching, Paused, Planning  }

public class ShowTracking : Entity
{
    public string UserRemoteId { get; set; }
    
    public string ShowRemoteId { get; set; }
    
    public int EpisodesWatched { get; set; }

    public ShowTrackingStatus Status { get; set; }
}