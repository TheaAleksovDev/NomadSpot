const _maps = new Map();
const _markers = new Map();

window.mapInterop = {
    initMap: function (element, lat, lng, dotNetRef) {
        requestAnimationFrame(() => requestAnimationFrame(() => {
            const map = L.map(element).setView([lat, lng], 13);
            L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
                attribution: '&copy; OpenStreetMap contributors'
            }).addTo(map);

            const marker = L.marker([lat, lng]).addTo(map);
            _maps.set(element, map);
            _markers.set(element, marker);

            map.on('click', function (e) {
                marker.setLatLng(e.latlng);
                dotNetRef.invokeMethodAsync('OnMapClick', e.latlng.lat, e.latlng.lng);
            });
        }));
    },

    updateMarker: function (element, lat, lng) {
        const tryUpdate = (attempts) => {
            const map = _maps.get(element);
            const marker = _markers.get(element);
            if (map && marker) {
                marker.setLatLng([lat, lng]);
                map.setView([lat, lng], 13);
            } else if (attempts > 0) {
                setTimeout(() => tryUpdate(attempts - 1), 100);
            }
        };
        tryUpdate(10);
    },

    getLocation: function (dotNetRef) {
        if (!navigator.geolocation) {
            dotNetRef.invokeMethodAsync('OnLocationError', 'Geolocation not supported');
            return;
        }
        navigator.geolocation.getCurrentPosition(
            pos => dotNetRef.invokeMethodAsync('OnLocationReceived',
                pos.coords.latitude, pos.coords.longitude),
            err => dotNetRef.invokeMethodAsync('OnLocationError', err.message)
        );
    }
};
