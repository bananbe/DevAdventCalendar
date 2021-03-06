using System;
using System.Reflection;
using AutoFixture.Kernel;

namespace DevAdventCalendarCompetition.Tests.AutoFixture
{
    public class TestNumberSpecimen : ISpecimenBuilder
    {
        private int _currentNumber = 100;

        public object Create(object request, ISpecimenContext context)
        {
            var propertyInfo = request as PropertyInfo;

            if (propertyInfo is null)
            {
                return new NoSpecimen();
            }

            var isNumberProperty = propertyInfo.Name.Contains("Number", StringComparison.InvariantCulture);
            var isIntProperty = propertyInfo.PropertyType == typeof(int);

            if (isNumberProperty && isIntProperty)
            {
                return this._currentNumber--;
            }

            return new NoSpecimen();
        }
    }
}
