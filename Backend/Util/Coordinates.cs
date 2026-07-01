namespace Backend.Util
{
	public class Coordinates
	{
		float lat, lng;

		public Coordinates(float lat, float lng)
		{
			this.lat = lat;
			this.lng = lng;
		}

		private static float squared(float x) => x * x;
		private static float ToRad(float degrees) => degrees * (MathF.PI / 180.0f);
		private static float hav(float x) => squared(MathF.Sin(x / 2));
		public static float geodesicDistance(Coordinates c1, Coordinates c2)
		{
			const float R = 6378.137f;

			float lat1 = ToRad(c1.lat);
			float lng1 = ToRad(c1.lng);
			float lat2 = ToRad(c2.lat);
			float lng2 = ToRad(c2.lng);

			float ht = hav(lat2 - lat1) + MathF.Cos(lat1) * MathF.Cos(lat2) * hav(lng2 - lng1);
			return 2 * R * MathF.Atan2(MathF.Sqrt(ht), MathF.Sqrt(1 - ht));
		}

		public static float[] CalculateDistances(Coordinates referenceLocation, List<Coordinates> locations)
		{
			float[] distances = new float[locations.Count];

			for (int i = 0; i < locations.Count; i++) {
				distances[i] = geodesicDistance(referenceLocation, locations[i]);
			}
			return distances;
		}

        public static float[][] CalculateDistanceMatrix(List<Coordinates> locations)
        {	
			int n = locations.Count;
            float[][] distances = new float[n][];
            for (int i = 0; i < n; i++)
            {
                distances[i] = new float[n];
            }

            for (int i = 0; i < n; i++)
            {
                for (int j = i + 1; j < n; j++)
                {
                    float dist = geodesicDistance(locations[i], locations[j]);
                    distances[i][j] = dist;
                    distances[j][i] = dist;
                }
            }

            return distances;
        }

    }
}
