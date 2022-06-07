﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using Moq;
using Npgsql;
using Piipan.States.Core.DataAccessObjects;
using Piipan.Shared.Database;
using Xunit;

namespace Piipan.States.Core.Integration.Tests
{
    [Collection("Core.IntegrationTests")]
    public class StateInfoDaoTests : DbFixture
    {
        private IDbConnectionFactory<StateInfoDb> DbConnFactory()
        {
            var factory = new Mock<IDbConnectionFactory<StateInfoDb>>();
            factory
                .Setup(m => m.Build(It.IsAny<string>()))
                .ReturnsAsync(() =>
                {
                    var conn = Factory.CreateConnection();
                    conn.ConnectionString = ConnectionString;
                    return conn;
                });

            return factory.Object;
        }

        [Fact]
        public async void GetStateByNameTest()
        {
            using (var conn = Factory.CreateConnection())
            {
                // Arrange
                conn.ConnectionString = ConnectionString;
                conn.Open();

                InsertState();

                var expected = GetLastStateName();

                var dao = new StateInfoDao(DbConnFactory());

                // Act
                var result = await dao.GetStateByName("test");

                // Assert
                Assert.Equal(expected, result.State);
            }
        }

        [Fact]
        public async void GetStateByIdTest()
        {
            using (var conn = Factory.CreateConnection())
            {
                // Arrange
                conn.ConnectionString = ConnectionString;
                conn.Open();

                InsertState();

                var expected = GetLastStateId();

                var dao = new StateInfoDao(DbConnFactory());

                // Act
                var result = await dao.GetStateById("99");

                // Assert
                Assert.Equal(expected, result.Id);
            }
        }
    }
}
