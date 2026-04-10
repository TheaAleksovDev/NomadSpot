using NomadSpot.Model.Entities;
using NomadSpot.Model.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace NomadSpot.ViewModel
{
    public class LocationViewModel : BaseViewModel
    {
        private readonly ILocationRepository _locationRepository;
        private readonly IReviewRepository _reviewRepository;

        private double _userLatitude;
        private double _userLongitude;
        private int _resultCount = 3;

        public double UserLatitude
        {
            get => _userLatitude;
            set => SetProperty(ref _userLatitude, value);
        }

        public double UserLongitude
        {
            get => _userLongitude;
            set => SetProperty(ref _userLongitude, value);
        }

        public int ResultCount
        {
            get => _resultCount;
            set => SetProperty(ref _resultCount, value);
        }

        public ListViewModel<Location> LocationList { get; } = new ListViewModel<Location>();
        public ListViewModel<Review> ReviewList { get; } = new ListViewModel<Review>();
        public FilterViewModel<IndoorLocation> IndoorFilter { get; } = new FilterViewModel<IndoorLocation>();
        public FilterViewModel<OutdoorLocation> OutdoorFilter { get; } = new FilterViewModel<OutdoorLocation>();

        public LocationViewModel(ILocationRepository locationRepository, IReviewRepository reviewRepository)
        {
            _locationRepository = locationRepository;
            _reviewRepository = reviewRepository;
        }

        public void LoadReviews(int locationId)
        {
            var reviews = _reviewRepository.GetByLocationId(locationId).ToList();
            ReviewList.SetItems(reviews);
            ReviewList.SetColumns(new[] { "Author", "Rating", "Date", "Comment" });
        }

        public void AddReview(Review review)
        {
            _reviewRepository.Add(review);

            var reviews = _reviewRepository.GetByLocationId(review.LocationId).ToList();

            var location = _locationRepository.GetById(review.LocationId);
            if (location != null)
            {
                location.Rating = reviews.Average(r => r.Rating);

                var noisyReviews = reviews.Where(r => r.NoiseLevel > 0).ToList();
                if (noisyReviews.Any())
                    location.NoiseLevel = (int)Math.Round(noisyReviews.Average(r => r.NoiseLevel));

                var wifiReviews = reviews.Where(r => r.WifiStrength > 0).ToList();
                if (wifiReviews.Any())
                    location.WifiStrength = (int)Math.Round(wifiReviews.Average(r => r.WifiStrength));

                if (location is Model.Entities.IndoorLocation indoor)
                {
                    var comfortReviews = reviews.Where(r => r.ComfortLevel > 0).ToList();
                    if (comfortReviews.Any())
                        indoor.ComfortLevel = (int)Math.Round(comfortReviews.Average(r => r.ComfortLevel));

                    var priceReviews = reviews.Where(r => r.PriceLevel > 0).ToList();
                    if (priceReviews.Any())
                        indoor.PriceLevel = (int)Math.Round(priceReviews.Average(r => r.PriceLevel));
                }
                else if (location is Model.Entities.OutdoorLocation outdoor)
                {
                    var cleanlinessReviews = reviews.Where(r => r.Cleanliness > 0).ToList();
                    if (cleanlinessReviews.Any())
                        outdoor.Cleanliness = (int)Math.Round(cleanlinessReviews.Average(r => r.Cleanliness));

                    var crowdednessReviews = reviews.Where(r => r.Crowdedness > 0).ToList();
                    if (crowdednessReviews.Any())
                        outdoor.Crowdedness = (int)Math.Round(crowdednessReviews.Average(r => r.Crowdedness));
                }

                _locationRepository.Update(location);
            }
        }

        public void SetInactive(int id)
        {
            _locationRepository.SetInactive(id);
        }

        public void AddLocation(Location location)
        {
            _locationRepository.Add(location);
        }

        public bool LastSearchWasIndoor { get; private set; }

        public void FindClosestLocations(bool indoorOnly)
        {
            LastSearchWasIndoor = indoorOnly;
            var filters = indoorOnly
                ? IndoorFilter.GetActiveFilters()
                : OutdoorFilter.GetActiveFilters();

            filters["LocationType"] = indoorOnly ? "Indoor" : "Outdoor";

            if (!filters.ContainsKey("IsActive"))
                filters["IsActive"] = true;

            var locations = _locationRepository.GetByFilter(filters)
                .OrderBy(l => CalculateDistance(l.Latitude, l.Longitude))
                .Take(ResultCount)
                .ToList();

            LocationList.SetItems(locations);
        }

        private double CalculateDistance(double lat, double lon)
        {
            const double R = 6371;
            var dLat = ToRad(lat - UserLatitude);
            var dLon = ToRad(lon - UserLongitude);
            var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                    Math.Cos(ToRad(UserLatitude)) * Math.Cos(ToRad(lat)) *
                    Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
            return R * 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        }

        private double ToRad(double deg) => deg * Math.PI / 180;
    }
}