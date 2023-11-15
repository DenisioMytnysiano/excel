using Excel.API.v1.Cells.Requests;
using FastEndpoints;
using FluentValidation;

namespace Excel.API.v1.Cells.Validators;

public class GetCellRequestValidator: Validator<GetCellRequest>
{
    public GetCellRequestValidator()
    {
        RuleFor(x => x.CellId)
            .NotEmpty()
            .WithMessage("Cell identifier should not be empty string.");
        
        RuleFor(x => x.SheetId)
            .NotEmpty()
            .WithMessage("Sheet identifier should not be empty string.");
    }
}