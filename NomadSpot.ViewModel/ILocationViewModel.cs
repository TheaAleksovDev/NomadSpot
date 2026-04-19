using System;

namespace NomadSpot.ViewModel
{
    public interface ILocationViewModel
    {
        event EventHandler LocationSaved;
        event EventHandler ReviewSaved;
    }
}
