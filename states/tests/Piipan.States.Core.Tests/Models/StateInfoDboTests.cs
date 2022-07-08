using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Piipan.States.Core.Models;

namespace Piipan.States.Core.Tests.Models
{
    public class StateInfoDboTests
    {
        [Fact]
        public void Equals_Match()
        {
            // Arrange
            var stateOne = new StateInfoDbo
            {
                Id = "1",
                State = "test",
                StateAbbreviation = "TT",
                Email = "test@email.example",
                Phone = "1234567890",
                Region = "TEST"
            };
            var stateTwo = new StateInfoDbo
            {
                Id = stateOne.Id,
                State = stateOne.State,
                StateAbbreviation = stateOne.StateAbbreviation,
                Email = stateOne.Email,
                Phone = stateOne.Phone,
                Region = stateOne.Region
            };


            // Act / Assert
            Assert.True(stateOne.Equals(stateTwo));
        }

        [Fact]
        public void Equals_IdMismatch()
        {
            // Arrange
            var stateOne = new StateInfoDbo
            {
                Id = "1",
                State = "test",
                StateAbbreviation = "TT",
                Email = "test@email.example",
                Phone = "123456789",
                Region = "TEST"
            };
            var stateTwo = new StateInfoDbo
            {
                Id = "2",
                State = stateOne.State,
                StateAbbreviation = stateOne.StateAbbreviation,
                Email = stateOne.Email,
                Phone = stateOne.Phone,
                Region = stateOne.Region
            };


            // Act / Assert
            Assert.False(stateOne.Equals(stateTwo));
        }

        [Fact]
        public void Equals_StateMismatch()
        {
            // Arrange
            var stateOne = new StateInfoDbo
            {
                Id = "1",
                State = "test",
                StateAbbreviation = "TT",
                Email = "test@email.example",
                Phone = "123456789",
                Region = "TEST"
            };
            var stateTwo = new StateInfoDbo
            {
                Id = stateOne.Id,
                State = "new",
                StateAbbreviation = stateOne.StateAbbreviation,
                Email = stateOne.Email,
                Phone = stateOne.Phone,
                Region = stateOne.Region
            };


            // Act / Assert
            Assert.False(stateOne.Equals(stateTwo));
        }

        [Fact]
        public void Equals_AbbreviationMismatch()
        {
            // Arrange
            var stateOne = new StateInfoDbo
            {
                Id = "1",
                State = "test",
                StateAbbreviation = "TT",
                Email = "test@email.example",
                Phone = "123456789",
                Region = "TEST"
            };
            var stateTwo = new StateInfoDbo
            {
                Id = stateOne.Id,
                State = stateOne.State,
                StateAbbreviation = "NEW",
                Email = stateOne.Email,
                Phone = stateOne.Phone,
                Region = stateOne.Region
            };


            // Act / Assert
            Assert.False(stateOne.Equals(stateTwo));
        }

        [Fact]
        public void Equals_EmailMismatch()
        {
            // Arrange
            var stateOne = new StateInfoDbo
            {
                Id = "1",
                State = "test",
                StateAbbreviation = "TT",
                Email = "test@email.example",
                Phone = "123456789",
                Region = "TEST"
            };
            var stateTwo = new StateInfoDbo
            {
                Id = stateOne.Id,
                State = stateOne.State,
                StateAbbreviation = stateOne.StateAbbreviation,
                Email = "new",
                Phone = stateOne.Phone,
                Region = stateOne.Region
            };


            // Act / Assert
            Assert.False(stateOne.Equals(stateTwo));
        }

        [Fact]
        public void Equals_PhoneMismatch()
        {
            // Arrange
            var stateOne = new StateInfoDbo
            {
                Id = "1",
                State = "test",
                StateAbbreviation = "TT",
                Email = "test@email.example",
                Phone = "123456789",
                Region = "TEST"
            };
            var stateTwo = new StateInfoDbo
            {
                Id = stateOne.Id,
                State = stateOne.State,
                StateAbbreviation = stateOne.StateAbbreviation,
                Email = stateOne.Email,
                Phone = "987654321",
                Region = stateOne.Region
            };


            // Act / Assert
            Assert.False(stateOne.Equals(stateTwo));
        }

        [Fact]
        public void Equals_RegionMismatch()
        {
            // Arrange
            var stateOne = new StateInfoDbo
            {
                Id = "1",
                State = "test",
                StateAbbreviation = "TT",
                Email = "test@email.example",
                Phone = "123456789",
                Region = "TEST"
            };
            var stateTwo = new StateInfoDbo
            {
                Id = stateOne.Id,
                State = stateOne.State,
                StateAbbreviation = stateOne.StateAbbreviation,
                Email = stateOne.Email,
                Phone = stateOne.Phone,
                Region = "NERO"
            };


            // Act / Assert
            Assert.False(stateOne.Equals(stateTwo));
        }

        [Fact]
        public void Equals_OverrideTests()
        {
            // Arrange
            var stateOne = new StateInfoDbo
            {
                Id = "1",
                State = "test",
                StateAbbreviation = "TT",
                Email = "test@email.example",
                Phone = "123456789",
                Region = "TEST"
            };


            // Act / Assert
            Assert.False(stateOne.Equals(null));
            Assert.False(stateOne.Equals("test"));
        }
    }
}
