namespace GoTournament.UnitTest
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using GoTournament.Service;
    using Xunit;

    public class JsonServiceTests
    {
        [Fact]
        public void DeserializeObjectTestMethod()
        {
            var json = new JsonService();
            var a = "\"2,33\"";
            var res = json.DeserializeObject<Point>(a);
            Assert.IsType(typeof(Point), res);
            Assert.Equal(2, res.X);
            Assert.Equal(33, res.Y);
        }

        [Fact]
        public void SerializeObjectTestMethod()
        {
            var json = new JsonService();
            var a = new
            {
                Name = "Yu",
                Forname = "Ri"
            };
            var res = json.SerializeObject(a);
            Assert.Equal("{\"Name\":\"Yu\",\"Forname\":\"Ri\"}", res);
            var point = new Point { X = 2, Y = 33 };
            res = json.SerializeObject(point);
            Assert.Equal("\"2, 33\"", res);
        }
    }
}
