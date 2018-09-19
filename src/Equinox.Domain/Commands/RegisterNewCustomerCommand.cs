using System;
using Equinox.Domain.Validations;

namespace Equinox.Domain.Commands
{
    //Para cada intenção de modificação no banco eu tenho um command
    public class RegisterNewCustomerCommand : CustomerCommand
    {
        public RegisterNewCustomerCommand(string name, string email, DateTime birthDate)
        {
            Name = name;
            Email = email;
            BirthDate = birthDate;
        }


        //Chama a classe que valida os parâmetros
        public override bool IsValid()
        {   //Esse this indica que estou querendo verificar a propria instância dessa classe.
            //Esse RegisterNewCustomerCommand valida a instância pelas regras de negócio.
            //Estou validando no command, pois a entidade pode mudar para cada situação.
            //Para cada command cria-se um commandvalidation
            ValidationResult = new RegisterNewCustomerCommandValidation().Validate(this);
            return ValidationResult.IsValid;
        }
    }
}