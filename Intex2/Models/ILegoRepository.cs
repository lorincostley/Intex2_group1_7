﻿namespace Intex2.Models
{
    public interface ILegoRepository
    {
        public IQueryable<Product> Products { get; }
    }
}
