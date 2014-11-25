﻿// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Threading.Tasks;
using Microsoft.AspNet.Mvc;
using Microsoft.AspNet.Mvc.ModelBinding;

namespace ModelBindingWebSite.Controllers
{
    public class TryUpdateModelController : Controller
    {
        public async Task<Person> GetPerson()
        {
            // Person has a property of type Person. Only Top level should be updated.
            var person = new Person();
            await TryUpdateModelAsync(
                person,
                prefix: string.Empty,
                includeExpressions: m => m.Parent);

            return person;
        }

        public async Task<User> GetUserAsync_IncludeAllByDefault(int id)
        {
            var user = GetUser(id);

            await TryUpdateModelAsync<User>(user, prefix: string.Empty);
            return user;
        }

        public async Task<User> GetUserAsync_ExcludeSpecificProperties(int id)
        {
            var user = GetUser(id);
            await TryUpdateModelAsync(
                user,
                prefix: string.Empty,
                predicate: 
                (context, modelName) => 
                    !string.Equals(modelName, nameof(ModelBindingWebSite.User.Id),StringComparison.Ordinal) &&
                    !string.Equals(modelName, nameof(ModelBindingWebSite.User.Key), StringComparison.Ordinal));

            return user;
        }

        public async Task<bool> CreateAndUpdateUser()
        {
            // don't update the id.
            var user = new User();
            return await TryUpdateModelAsync(user,
                                             prefix: string.Empty,
                                             includeExpressions: model => model.RegisterationMonth);
        }

        public async Task<User> GetUserAsync_IncludeSpecificProperties(int id)
        {
            var user = GetUser(id);
            await TryUpdateModelAsync(user,
                                      prefix: string.Empty,
                                      includeExpressions: model => model.RegisterationMonth);

            return user;
        }

        public async Task<bool> TryUpdateModelFails(int id)
        {
            var user = GetUser(id);
            return await TryUpdateModelAsync(user,
                                             prefix: string.Empty,
                                             valueProvider: new CustomValueProvider());
        }

        public async Task<User> GetUserAsync_IncludeAndExcludeListNull(int id)
        {
            var user = GetUser(id);
            await TryUpdateModelAsync(user);

            return user;
        }

        public async Task<User> GetUserAsync_IncludesAllSubProperties(int id)
        {
            var user = GetUser(id);

            await TryUpdateModelAsync(user, prefix: string.Empty, includeExpressions: model => model.Address);

            return user;
        }

        public async Task<User> GetUserAsync_WithChainedProperties(int id)
        {
            var user = GetUser(id);

            // Since this is a chained expression this would throw
            await TryUpdateModelAsync(user, prefix: string.Empty, includeExpressions: model => model.Address.Country);

            return user;
        }

        private User GetUser(int id)
        {
            return new User
            {
                UserName = "User_" + id,
                Id = id,
                Key = id + 20,
            };
        }

        public class CustomValueProvider : IValueProvider
        {
            public Task<bool> ContainsPrefixAsync(string prefix)
            {
                return Task.FromResult(false);
            }

            public Task<ValueProviderResult> GetValueAsync(string key)
            {
                return Task.FromResult<ValueProviderResult>(null);
            }
        }
    }
}