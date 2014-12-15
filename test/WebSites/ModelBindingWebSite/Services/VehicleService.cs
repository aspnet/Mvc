﻿// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using ModelBindingWebSite.ViewModels;

namespace ModelBindingWebSite.Services
{
    public class VehicleService : IVehicleService
    {
        public void Update(int id, VehicleViewModel vehicle, string trackingId)
        {
            if (trackingId == null)
            {
                throw new ArgumentException(nameof(trackingId));
            }

            vehicle.LastUpdatedTrackingId = trackingId;
        }
    }
}