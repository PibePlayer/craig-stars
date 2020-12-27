namespace CraigStars {
    public static class GameUtils {
        /// <summary>
        /// Get the distance traveled for a given warp speed
        /// </summary>
        /// <param name="warp"></param>
        /// <returns></returns>
        public static int GetDistanceTravelled(int warpFactor) {
            return warpFactor * warpFactor;
        }
    }
}