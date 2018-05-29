// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;

namespace BasicApi.Models
{
    public class PetRepository
    {
        static PetRepository()
        {
            Categories = new List<Category>()
            {
                new Category() { Id = 1, Name = "Dogs", },
                new Category() { Id = 2, Name = "Cats", },
                new Category() { Id = 3, Name = "Rabbits", },
                new Category() { Id = 4, Name = "Lions", },
            };

            Tags = new List<Tag>()
            {
                new Tag() { Id = 1, Name = "tag1", },
                new Tag() { Id = 2, Name = "tag2", },
            };

            Pets = new List<Pet>()
            {
                new Pet() { Id = 1, Category = Categories[0], Name = "Cat 1", Status = "available", Tags = Tags, Images = new List<Image>() { new Image { Url = "url1"}, new Image { Url = "url2" } } },
                new Pet() { Id = 2, Category = Categories[0], Name = "Cat 2", Status = "available", Tags = Tags, Images = new List<Image>() { new Image { Url = "url1"}, new Image { Url = "url2" } } },
                new Pet() { Id = 3, Category = Categories[0], Name = "Cat 3", Status = "available", Tags = Tags, Images = new List<Image>() { new Image { Url = "url1"}, new Image { Url = "url2" } } },
                new Pet() { Id = 4, Category = Categories[1], Name = "Dog 1", Status = "available", Tags = Tags, Images = new List<Image>() { new Image { Url = "url1"}, new Image { Url = "url2" } } },
                new Pet() { Id = 5, Category = Categories[1], Name = "Dog 2", Status = "available", Tags = Tags, Images = new List<Image>() { new Image { Url = "url1"}, new Image { Url = "url2" } } },
                new Pet() { Id = 6, Category = Categories[1], Name = "Dog 3", Status = "available", Tags = Tags, Images = new List<Image>() { new Image { Url = "url1"}, new Image { Url = "url2" } } },
                new Pet() { Id = 7, Category = Categories[2], Name = "Rabbit 1", Status = "available", Tags = Tags, Images = new List<Image>() { new Image { Url = "url1"}, new Image { Url = "url2" } } },
                new Pet() { Id = 8, Category = Categories[2], Name = "Rabbit 1", Status = "available", Tags = Tags, Images = new List<Image>() { new Image { Url = "url1"}, new Image { Url = "url2" } } },
                new Pet() { Id = 9, Category = Categories[2], Name = "Rabbit 1", Status = "available", Tags = Tags, Images = new List<Image>() { new Image { Url = "url1"}, new Image { Url = "url2" } } },
                new Pet() { Id = 10, Category = Categories[3], Name = "Lion 1", Status = "available", Tags = Tags, Images = new List<Image>() { new Image { Url = "url1"}, new Image { Url = "url2" } } },
                new Pet() { Id = 11, Category = Categories[3], Name = "Lion 1", Status = "available", Tags = Tags, Images = new List<Image>() { new Image { Url = "url1"}, new Image { Url = "url2" } } },
                new Pet() { Id = 12, Category = Categories[3], Name = "Lion 1", Status = "available", Tags = Tags, Images = new List<Image>() { new Image { Url = "url1"}, new Image { Url = "url2" } } },
            };
        }

        public static List<Category> Categories { get; set; }
        public static List<Pet> Pets { get; set; }
        public static List<Tag> Tags { get; set; }

        public void AddPet(Pet pet)
        {
            // Do nothing
        }

        public Pet FindPet(int id)
        {
            for (var i = 0; i < Pets.Count; i++)
            {
                var pet = Pets[i];
                if (pet.Id == id)
                {
                    return pet;
                }
            }

            return null;
        }
    }
}
