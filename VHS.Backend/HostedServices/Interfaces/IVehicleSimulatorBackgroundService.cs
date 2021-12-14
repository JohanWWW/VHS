﻿using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VHS.Utility.Types;

namespace VHS.Backend.HostedServices.Interfaces
{
    public delegate void PositionUpdatedEventHandler(GeoCoordinate? currentPosition);
    public delegate void DistanceUpdatedEventHandler(DistanceEventArgs distanceArgs);
    public class DistanceEventArgs : EventArgs
    {
        public double Distance { get; }
        public double TotalDistance { get; }

        public DistanceEventArgs(double distance, double totalDistance) { Distance = distance; TotalDistance = totalDistance;}
    }
    public interface IVehicleSimulatorBackgroundService
    {
        double Distance { get; }
        GeoCoordinate? Position { get; }
        public event DistanceUpdatedEventHandler DistanceUpdated;


        public event PositionUpdatedEventHandler PositionUpdated;
    }
}
