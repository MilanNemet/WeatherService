using System.Text;

using WeatherService.Boundary;
using WeatherService.Entity;

namespace WeatherService.Control
{
    class UiSourceBuilder
    {
        private readonly Region _local;
        private readonly Region _remote;
        private readonly FileManager _fm;
        public UiSourceBuilder(Region local, Region remote, FileManager fm)
        {
            _local = local;
            _remote = remote;
            _fm = fm;
        }

        public void Build()
        {
            //let dates;
            //let maxValues;
            //let progMaxValues;
            //let minValues;
            //let progMinValues;
            var datesBuilder = new StringBuilder();
            var maxValuesBuilder = new StringBuilder();
            var progMaxValuesBuilder = new StringBuilder();
            var minValuesBuilder = new StringBuilder();
            var progMinValuesBuilder = new StringBuilder();


        }
    }
}
