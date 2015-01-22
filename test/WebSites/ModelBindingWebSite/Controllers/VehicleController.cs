// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net;
using Microsoft.AspNet.Mvc;
using ModelBindingWebSite.Services;
using ModelBindingWebSite.ViewModels;

namespace ModelBindingWebSite
{
    public class VehicleController : Controller
    {
        private static Dictionary<int, VehicleViewModel> _vehicles = new Dictionary<int, VehicleViewModel>
        {
            {
                42,
                new VehicleViewModel
                {
                    InspectedDates = new[] { DateTimeOffset.Parse("1 April 2001"), },
                    Make = "Fast Cars",
                    Model = "the Fastener",
                    Vin = "87654321",
                    Year = 2013,
                }
            },
        };

        [HttpPut("/api/vehicles/{id}")]
        [Produces("application/json")]
        public object UpdateVehicleApi(
            [Range(1, 500)] int id,
            [FromBody] VehicleViewModel model,
            [FromServices] IVehicleService service,
            [FromHeader(Name = "X-TrackingId")] string trackingId)
        {
            if (!ModelState.IsValid)
            {
                return SerializeModelState();
            }

            service.Update(id, model, trackingId);

            return model;
        }

        [HttpPost("/dealers/{dealer.id:int}/update-vehicle")]
        public IActionResult UpdateDealerVehicle(VehicleWithDealerViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return PartialView("UpdateVehicle", model);
            }

            model.Update();
            return PartialView("UpdateSuccessful", model);
        }

        [HttpGet("/vehicles/{id:int}")]
        public IActionResult Details(int id)
        {
            VehicleViewModel vehicle;
            if (!_vehicles.TryGetValue(id, out vehicle))
            {
                return HttpNotFound();
            }

            return View(vehicle);
        }

        [HttpGet("/vehicles/{id:int}/edit")]
        public IActionResult Edit(int id)
        {
            VehicleViewModel vehicle;
            if (!_vehicles.TryGetValue(id, out vehicle))
            {
                return HttpNotFound();
            }

            // Provide room for one additional inspection if not already full.
            var length = vehicle.InspectedDates.Length;
            if (length < 10)
            {
                var array = new DateTimeOffset[length + 1];
                vehicle.InspectedDates.CopyTo(array, 0);

                // Don't update the stored VehicleViewModel instance.
                vehicle = new VehicleViewModel
                {
                    InspectedDates = array,
                    LastUpdatedTrackingId = vehicle.LastUpdatedTrackingId,
                    Make = vehicle.Make,
                    Model = vehicle.Model,
                    Vin = vehicle.Vin,
                    Year = vehicle.Year,
                };
            }

            return View(vehicle);
        }

        [HttpPost("/vehicles/{id:int}/edit")]
        public IActionResult Edit(int id, VehicleViewModel vehicle)
        {
            if (!_vehicles.ContainsKey(id))
            {
                return HttpNotFound();
            }

            if (!ModelState.IsValid)
            {
                return View(vehicle);
            }

            if (vehicle.InspectedDates != null)
            {
                // Ignore empty inspection values.
                var nonEmptyDates = vehicle.InspectedDates.Where(date => date != default(DateTimeOffset)).ToArray();
                vehicle.InspectedDates = nonEmptyDates;
            }

            _vehicles[id] = vehicle;

            return RedirectToAction(nameof(Details), new { id = id });
        }

        public IDictionary<string, IEnumerable<string>> SerializeModelState()
        {
            Response.StatusCode = (int)HttpStatusCode.BadRequest;

            return ModelState.Where(item => item.Value.Errors.Count > 0)
                             .ToDictionary(item => item.Key, item => item.Value.Errors.Select(e => e.ErrorMessage));
        }
    }
}