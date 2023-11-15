using Excel.API.v1.Cells.Requests;
using FastEndpoints;
using FluentValidation;

namespace Excel.API.v1.Cells.Validators;

public class AddCellRequestValidator: Validator<AddCellRequest>
{
    public AddCellRequestValidator()
    {
        RuleFor(x => x.CellId)
            .NotEmpty()
            .WithMessage("Cell identifier should not be empty string.");
        
        RuleFor(x => x.SheetId)
            .NotEmpty()
            .WithMessage("Sheet identifier should not be empty string.");
        
        RuleFor(x => x.Value)
            .NotEmpty()
            .WithMessage("Cell value should not be empty.")
            .NotNull()
            .WithMessage("Cell value should not bu null");
    }
}