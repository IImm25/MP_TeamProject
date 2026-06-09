export interface Turbine {
  id: number;
  name: string;
  latitude: number;
  longitude: number;
}

export interface CreateTurbine {
  name: string;
  latitude: number;
  longitude: number;
}
