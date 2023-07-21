using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BrainstormSessions.Api;
using BrainstormSessions.Controllers;
using BrainstormSessions.Core.Interfaces;
using BrainstormSessions.Core.Model;
using log4net.Appender;
using log4net.Config;
using log4net.Core;
using Moq;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;
using Xunit;
using Serilog.Sinks.InMemory;
using Serilog.Core;

namespace BrainstormSessions.Test.UnitTests
{
    public class LoggingTests : IDisposable
    {
        private readonly MemoryAppender _appender;

        //test way
        private readonly List<LogEvent> _logEvents;

        public LoggingTests()
        {
            _appender = new MemoryAppender();
            BasicConfigurator.Configure(_appender);

            //test way
            _logEvents = new List<LogEvent>();
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.Sink(new TestSink(_logEvents))
                .CreateLogger();
        }

        public void Dispose()
        {
            _appender.Clear();
        }

        [Fact]
        public async Task HomeController_Index_LogInfoMessages()
        {
            // Arrange
            var mockRepo = new Mock<IBrainstormSessionRepository>();
            mockRepo.Setup(repo => repo.ListAsync())
                .ReturnsAsync(GetTestSessions());
            var controller = new HomeController(mockRepo.Object);

            // Act
            var result = await controller.Index();

            // Assert
            //var logEntries = _appender.GetEvents();
            //Assert.True(logEntries.Any(l => l.Level == Level.Info), "Expected Info messages in the logs");


            //Assert another way
            Assert.True(_logEvents.Any(logEvent => logEvent.Level == LogEventLevel.Information),
                "Expected Info messages in the logs");
        }

        [Fact]
        public async Task HomeController_IndexPost_LogWarningMessage_WhenModelStateIsInvalid()
        {
            // Arrange
            var mockRepo = new Mock<IBrainstormSessionRepository>();
            mockRepo.Setup(repo => repo.ListAsync())
                .ReturnsAsync(GetTestSessions());
            var controller = new HomeController(mockRepo.Object);
            controller.ModelState.AddModelError("SessionName", "Required");
            var newSession = new HomeController.NewSessionModel();

            // Act
            var result = await controller.Index(newSession);

            // Assert
            var logEntries = _appender.GetEvents();
            Assert.True(logEntries.Any(l => l.Level == Level.Warn), "Expected Warn messages in the logs");
        }

        [Fact]
        public async Task IdeasController_CreateActionResult_LogErrorMessage_WhenModelStateIsInvalid()
        {
            // Arrange & Act
            var mockRepo = new Mock<IBrainstormSessionRepository>();
            var controller = new IdeasController(mockRepo.Object);
            controller.ModelState.AddModelError("error", "some error");

            // Act
            var result = await controller.CreateActionResult(model: null);

            // Assert
            var logEntries = _appender.GetEvents();
            Assert.True(logEntries.Any(l => l.Level == Level.Error), "Expected Error messages in the logs");
        }

        [Fact]
        public async Task SessionController_Index_LogDebugMessages()
        {
            // Arrange
            int testSessionId = 1;
            var mockRepo = new Mock<IBrainstormSessionRepository>();
            mockRepo.Setup(repo => repo.GetByIdAsync(testSessionId))
                .ReturnsAsync(GetTestSessions().FirstOrDefault(
                    s => s.Id == testSessionId));
            var controller = new SessionController(mockRepo.Object);

            // Act
            var result = await controller.Index(testSessionId);

            // Assert
            var logEntries = _appender.GetEvents();
            Assert.True(logEntries.Count(l => l.Level == Level.Debug) == 2, "Expected 2 Debug messages in the logs");
        }

        private static List<BrainstormSession> GetTestSessions()
        {
            var sessions = new List<BrainstormSession>
            {
                new BrainstormSession()
                {
                    DateCreated = new DateTime(2016, 7, 2),
                    Id = 1,
                    Name = "Test One"
                },
                new BrainstormSession()
                {
                    DateCreated = new DateTime(2016, 7, 1),
                    Id = 2,
                    Name = "Test Two"
                }
            };
            return sessions;
        }

        private class TestSink : ILogEventSink
        {
            private readonly List<LogEvent> _logEvents;

            public TestSink(List<LogEvent> logEvents)
            {
                _logEvents = logEvents;
            }

            public void Emit(LogEvent logEvent)
            {
                _logEvents.Add(logEvent);
            }
        }

    }
}
