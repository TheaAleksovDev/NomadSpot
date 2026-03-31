using NomadSpot.Model.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace NomadSpot.Model.Repositories
{
    public interface IReviewRepository
    {
        IEnumerable<Review> GetByLocationId(int locationId);
        void Add(Review review);
    }
}
