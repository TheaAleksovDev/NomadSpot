using NomadSpot.Model.Entities;
using NomadSpot.Model.Repositories;
using NomadSpot.ViewModel.NomadSpot.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;

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
        public FilterViewModel<IndoorLocation> IndoorFilter { get; } = new FilterViewModel<IndoorLocation>();
        public FilterViewModel<OutdoorLocation> OutdoorFilter { get; } = new FilterViewModel<OutdoorLocation>();

        public LocationViewModel(ILocationRepository locationRepository, IReviewRepository reviewRepository)
        {
            _locationRepository = locationRepository;
            _reviewRepository = reviewRepository;
        }

        public void AddLocation(Location location)
        {
            _locationRepository.Add(location);
        }

        public void FindClosestLocations(bool indoorOnly)
        {
            var filters = indoorOnly
                ? IndoorFilter.GetActiveFilters()
                : OutdoorFilter.GetActiveFilters();

            var locations = _locationRepository.GetByFilter(filters)
                .OrderBy(l => CalculateDistance(l.Latitude, l.Longitude))
                .Take(ResultCount);

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