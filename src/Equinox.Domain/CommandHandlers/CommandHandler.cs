using Equinox.Domain.Core.Bus;
using Equinox.Domain.Core.Commands;
using Equinox.Domain.Core.Notifications;
using Equinox.Domain.Interfaces;
using MediatR;

namespace Equinox.Domain.CommandHandlers
{
    public class CommandHandler
    {
        private readonly IUnitOfWork _uow;
        private readonly IMediatorHandler _bus;
        private readonly DomainNotificationHandler _notifications;

        public CommandHandler(IUnitOfWork uow, IMediatorHandler bus, INotificationHandler<DomainNotification> notifications)
        {
            _uow = uow;
            _notifications = (DomainNotificationHandler)notifications;
            _bus = bus;
        }

        protected void NotifyValidationErrors(Command message)
        {
            foreach (var error in message.ValidationResult.Errors)
            {
                _bus.RaiseEvent(new DomainNotification(message.MessageType, error.ErrorMessage));
            }
        }

        public bool Commit()
        {   
            //Se o banco retornar algo de errado
            if (_notifications.HasNotifications()) return false;
            //Se o banco inserir normalmente
            if (_uow.Commit()) return true;

            //Caso contrário lançar um evento de erro
            _bus.RaiseEvent(new DomainNotification("Commit", "We had a problem during saving your data."));
            return false;
        }
    }
}