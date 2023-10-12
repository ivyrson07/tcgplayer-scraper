using TCG.Scraper.Sellers.Models;

namespace TCG.Scraper.Sellers.Helpers
{
    public class TaskSegment
    {
        public static List<Segment> GetSegments(int total, int segmentSize)
        {
            List<Segment> segments = new List<Segment>();

            var numberOfSegments = total / segmentSize;
            var completed = false;
            var i = 0;

            do
            {
                int min = i * numberOfSegments + 1;
                int max = (i + 1) * numberOfSegments;

                // Adjust the max value if it exceeds the total
                if (max > total)
                {
                    max = total;
                    completed = true;
                }

                segments.Add(new Segment
                {
                    Minimum = min,
                    Maximum = max,
                });

                i++;
            }
            while (!completed);

            return segments;
        }
    }
}
