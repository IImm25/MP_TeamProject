import { AfterViewInit, Component, inject, signal } from '@angular/core';
import * as L from 'leaflet';
import { HttpService } from '../../../Services/http-service'; import { Turbine } from '../../../Models/turbine';
import { Observable } from 'rxjs';
import { DialogTurbine } from '../dialog-turbine/dialog-turbine';
;

@Component({
  selector: 'app-map',
  imports: [DialogTurbine],
  templateUrl: './map.html',
  styleUrl: './map.css',
})
export class Map implements AfterViewInit {

  private map!: L.Map;
  private markerLayer!: L.LayerGroup;
  private http = inject(HttpService);
  private turbines = signal<Turbine[]>([]);
  visible = signal<boolean>(false);
  selectedTurbine = signal<Turbine | null>(null);

  private static turbineIcon = L.icon({
    iconUrl: 'logo.png',
    iconSize: [32, 32],
    iconAnchor: [16, 32],
    popupAnchor: [0, -32]
  });

  private static harbour: L.LatLngExpression = [54.433304330384395, 13.031369793515506];

  ngAfterViewInit(): void {
    this.initMap();
  }

  private initMap(): void {
    this.map = L.map('map').setView([54.61092, 12.63], 13);
    this.markerLayer = L.layerGroup();

    this.markerLayer.addTo(this.map);

    this.http.getAllTurbines().subscribe((turbines) => {
      this.turbines.set(turbines);
      this.updateMarkers();
    });

    L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
      attribution: '&copy; OpenStreetMap contributors'
    }).addTo(this.map);

  }

  private updateMarkers() {
    this.markerLayer.clearLayers();

    this.turbines().forEach(turbine => {

      const marker = L.marker([turbine.latitude, turbine.longitude], { icon: Map.turbineIcon }) // lat first!
        .addTo(this.markerLayer);
      
      marker.addEventListener("click", () => {
        this.selectedTurbine.set(turbine);
        this.visible.set(true);
      });

    });

    L.marker(Map.harbour) // lat first!
      .bindPopup("Barhöft Hafen")
      .addTo(this.markerLayer);
  }


}
