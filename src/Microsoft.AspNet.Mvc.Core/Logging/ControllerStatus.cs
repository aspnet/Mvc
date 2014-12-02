// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.AspNet.Mvc.Logging
{
    /// <summary>
    /// Indicates the status of a class during controller discovery. 
    /// All values except 0 represent a reason why a type is not a controller.
    /// </summary>
    public enum ControllerStatus
    {
        IsController,
        NotAnEligibleClass,
        // The name of the controller class is "Controller"
        NameIsController,
        DoesNotEndWithControllerAndIsNotAssignable
    }
}