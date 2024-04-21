
using CarrotSystem.Models.MPS;

namespace CarrotSystem.Services
{
    public interface IEventWriter
    {
        void WriteEvents(string eventBy, string eventType, string actionType, string eventDesc);
        void WriteEventOnly(string eventBy, string eventType, string actionType, string eventDesc);
        void WriteTestLog(string content);
        void WriteLogOnly(string eventBy, string eventType, string eventDesc, string logLevel);
    }

    public class EventWriter : IEventWriter
    {
        private readonly ILogger<EventWriter> _logger;
        private readonly IContextService _context;
        
        public EventWriter(ILogger<EventWriter> logger, IContextService context)
        {
            _logger = logger;
            _context = context;
        }

        // Impl methods

        public void WriteEvents(string eventBy, string eventType, string actionType, string eventDesc)
        {
            WriteEventLog(eventBy, eventType, actionType, eventDesc);
            return;
        }

        public void WriteLogOnly(string eventBy, string eventType, string eventDesc, string logLevel)
        {
            WriteLog(eventBy, eventType, eventDesc, logLevel);
            return;
        }

        public void WriteEventOnly(string eventBy, string eventType, string actionType, string eventDesc)
        {
            WriteEvent(eventBy, eventType, actionType, eventDesc);
            return;
        }

        public void WriteTestLog(string content)
        {
            _logger.LogError(content);
        }

        // Internal methods

        private void WriteEventLog(string eventBy, string eventType, string actionType, string eventDesc)
        {
            WriteEvent(eventBy, eventType, actionType, eventDesc);
            
            WriteTestLog(eventBy + ", " + eventType + ", " + actionType + " , " + eventDesc);

            return;
        }

        private void WriteLog(string eventBy, string eventType, string eventDesc, string logLevel)
        {
            if(logLevel.Equals("Info"))
            {
                _logger.LogInformation(eventType + ", "+ eventDesc+" by "+ eventBy);
            }
            else if(logLevel.Equals("Test"))
            {
                _logger.LogError(eventDesc);
            }
            else
            {
                _logger.LogError(eventType + ", " + eventDesc + " by " + eventBy);
            }

            return;
        }

        private void WriteEvent(string eventBy, string eventType, string actionType, string eventDesc)
        {
            var newEvent = new Event();

            newEvent.EventBy = eventBy;
            newEvent.EventType = eventType;
            newEvent.ActionType = actionType;
            newEvent.EventDesc = eventDesc;

            _context.GetMPSContext().Events.Add(newEvent);
            _context.GetMPSContext().SaveChanges();
            
            return;
        }

    }
}
