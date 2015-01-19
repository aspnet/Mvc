// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.AspNet.Mvc.ModelBinding;

namespace Microsoft.AspNet.Mvc
{
    /// <summary>
    /// This attribute is used on action parameters or model properties to indicate that
    /// they will be bound with the <see cref="ServicesModelBinder"/> using the <see cref="IServiceProvider"/>.
    /// </summary>
    /// <example>  
    /// In this example, the LocationService property on the VehicleWithDealerViewModel class 
    /// will be bound to the value resolved for the ILocationService service from IoC.
    ///
    /// <code> 
    /// public class VehicleWithDealerViewModel 
    /// {
    /// 	[Required]
    /// 	public DealerViewModel Dealer { get; set; }
    /// 
    /// 	[Required]
    /// 	[FromBody]
    /// 	public VehicleViewModel Vehicle { get; set; }
    /// 
    /// 	[FromServices]
    /// 	public ILocationService LocationService { get; set; }
    /// 
    /// 	[FromHeader(Name = "X-TrackingId")]
    /// 	public string TrackingId { get; set; } = "default-tracking-id";
    /// 
    /// 	public void Update()
    /// 	{
    /// 		LocationService.Update(this);
    /// 	}
    /// }
    /// </code> 
    ///
    /// In this example an implementation of IProductModelRequestService is registered in IoC. Then in the GetProduct action, 
    /// the parameter is bound to an instance of IProductModelRequestService which is resolved from the IoC container.
    ///
    /// <code>
    /// public class ProductModelRequestService : IProductModelRequestService
    ///	{
    ///		public ProductModel(IContextAccessor<ActionContext> action, IProductService prodService)
    ///		{
    ///			if (!action.Value.RouteData.Values.ContainsKey("product")) 
    ///				throw new InvalidOperationException("The 'product' key was not available in the request");	
    ///			Value = prodService.Get(action.Value.RouteData.Values["product"]);
    ///		}
    ///
    ///		public ProductModel Value { get; private set; }
    ///	}
    /// </code>
    ///
    ///	<code>
    /// [HttpGet]
    ///	public ProductModel GetProduct(
    ///		[FromServices]IProductModelRequestService productModelReqest)
    ///		{
    ///			return productModelReqest.Value;
    ///		}
    /// </code>
    ///
    /// </example> 
    [AttributeUsage(AttributeTargets.Parameter | AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class FromServicesAttribute : Attribute, IServiceActivatorBinderMetadata
    {
    }
}
