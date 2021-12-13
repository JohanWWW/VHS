using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VHS.Utility.Types;

namespace VHS.Backend.HostedServices.Interfaces
{
    public delegate void PositionUpdatedEventHandler(GeoCoordinate? currentPosition);

    public interface IVehicleSimulatorBackgroundService
    {
        GeoCoordinate? Position { get; }

        public event PositionUpdatedEventHandler PositionUpdated;
    }
}
