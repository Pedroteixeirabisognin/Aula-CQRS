using Equinox.Domain.Commands;

namespace Equinox.Domain.Validations
{
    //As validações são feitar em uma classe abstrata que recebe um genérico para 
    //não precisar ficar implementando mais de uma vez a mesma validação. Como ela já herda de CustomerValidation só necessitamos
    //chamar os métodos
    public class RegisterNewCustomerCommandValidation : CustomerValidation<RegisterNewCustomerCommand>
    {
        public RegisterNewCustomerCommandValidation()
        {   
            ValidateName();
            ValidateBirthDate();
            ValidateEmail();
        }
    }
}