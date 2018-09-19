using System;
using System.Threading;
using System.Threading.Tasks;
using Equinox.Domain.Commands;
using Equinox.Domain.Core.Bus;
using Equinox.Domain.Core.Notifications;
using Equinox.Domain.Events;
using Equinox.Domain.Interfaces;
using Equinox.Domain.Models;
using MediatR;

//Uma classe Handler pode implementar mais de um command, mas para cada command precisará ter um handler pra ele
namespace Equinox.Domain.CommandHandlers
{
    public class CustomerCommandHandler : CommandHandler,
        IRequestHandler<RegisterNewCustomerCommand>,
        IRequestHandler<UpdateCustomerCommand>,
        IRequestHandler<RemoveCustomerCommand>
    {
        private readonly ICustomerRepository _customerRepository;
        private readonly IMediatorHandler Bus;

        public CustomerCommandHandler(ICustomerRepository customerRepository, 
                                      IUnitOfWork uow,
                                      IMediatorHandler bus,
                                      INotificationHandler<DomainNotification> notifications) :base(uow, bus, notifications)
        {
            _customerRepository = customerRepository;
            Bus = bus;
        }

        //Por exemplo em RegisterNewCustomer
        public Task Handle(RegisterNewCustomerCommand message, CancellationToken cancellationToken)
        {   //Iremos validar o comando é valido, se não for vamos enviar uma notificação e não uma exceção
            if (!message.IsValid())
            {
                NotifyValidationErrors(message);
                return Task.CompletedTask;
            }
            //Se o registro for válido criaremos uma variável customer que irá receber a instância da entidade
            //Posso usar o automapper ou fazer na mão
            var customer = new Customer(Guid.NewGuid(), message.Name, message.Email, message.BirthDate);

            //Validamos depois se essa instância já não existe no banco de dados
            if (_customerRepository.GetByEmail(customer.Email) != null)
            {
                Bus.RaiseEvent(new DomainNotification(message.MessageType, "The customer e-mail has already been taken."));
                return Task.CompletedTask;
            }
            
            //Aqui ele está usando o UnityOffWork, só adicionou no repositório, mas não salvou no banco
            _customerRepository.Add(customer);

            //Fez o commit, nesse método commit ele faz a validação do commit
            if (Commit())
            {
                Bus.RaiseEvent(new CustomerRegisteredEvent(customer.Id, customer.Name, customer.Email, customer.BirthDate));
            }

            return Task.CompletedTask;
        }

        public Task Handle(UpdateCustomerCommand message, CancellationToken cancellationToken)
        {
            if (!message.IsValid())
            {
                NotifyValidationErrors(message);
                return Task.CompletedTask;
            }

            var customer = new Customer(message.Id, message.Name, message.Email, message.BirthDate);
            var existingCustomer = _customerRepository.GetByEmail(customer.Email);

            if (existingCustomer != null && existingCustomer.Id != customer.Id)
            {
                if (!existingCustomer.Equals(customer))
                {
                    Bus.RaiseEvent(new DomainNotification(message.MessageType,"The customer e-mail has already been taken."));
                    return Task.CompletedTask;
                }
            }

            _customerRepository.Update(customer);

            if (Commit())
            {
                Bus.RaiseEvent(new CustomerUpdatedEvent(customer.Id, customer.Name, customer.Email, customer.BirthDate));
            }

            return Task.CompletedTask;
        }

        public Task Handle(RemoveCustomerCommand message, CancellationToken cancellationToken)
        {
            if (!message.IsValid())
            {
                NotifyValidationErrors(message);
                return Task.CompletedTask;
            }

            _customerRepository.Remove(message.Id);

            if (Commit())
            {
                Bus.RaiseEvent(new CustomerRemovedEvent(message.Id));
            }

            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _customerRepository.Dispose();
        }
    }
}