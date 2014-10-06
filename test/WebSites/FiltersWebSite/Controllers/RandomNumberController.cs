﻿// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNet.Mvc;

namespace FiltersWebSite
{
    public class RandomNumberController : Controller
    {
        [ServiceFilter(typeof(RandomNumberFilter))]
        public int GetRandomNumber()
        {
            return 2;
        }

        [ServiceFilter(typeof(FakeUserAttribute))]
        public int GetFakeRandomNumber()
        {
            return 2;
        }

        [TypeFilter(typeof(RandomNumberProvider))]
        public int GetModifiedRandomNumber(int randomNumber)
        {
            return randomNumber / 2;
        }

        [TypeFilter(typeof(FakeRandomNumberProvider))]
        public int GetFakeModifiedRandomNumber(int randomNumber)
        {
            return randomNumber / 2;
        }
    }
}