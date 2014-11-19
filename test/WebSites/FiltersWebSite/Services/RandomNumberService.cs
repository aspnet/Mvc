// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace FiltersWebSite
{
    public class RandomNumberService
    {
        public int GetRandomNumber()
        {
            return 44;
        }

        public int GetRandomNumber(int minValue, int maxValue) {
            return new Random().Next(minValue, maxValue);
        }
    }
}