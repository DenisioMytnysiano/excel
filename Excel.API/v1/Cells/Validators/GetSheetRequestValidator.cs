using Excel.API.v1.Cells.Requests;
using FastEndpoints;
using FluentValidation;

namespace Excel.API.v1.Cells.Validators;

public class GetSheetRequestValidator: Validator<GetSheetRequest>
{
    public GetSheetRequestValidator()
    {
        RuleFor(x => x.SheetId)
            .NotEmpty()
            .WithMessage("Sheet identifier should not be empty string.");
    }
}