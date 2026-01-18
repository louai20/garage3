using garage3.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;

namespace garage3.Validation
{
	public class ApplicationUserValidator : IUserValidator<ApplicationUser>
	{
		// Regex to validate Swedish personal number format YYYYMMDD-XXXX
		private static readonly Regex PersonalNumberRegex =
			new(@"^\d{8}-\d{4}$", RegexOptions.Compiled);

		public async Task<IdentityResult> ValidateAsync(
			UserManager<ApplicationUser> manager,
			ApplicationUser user)
		{
			var errors = new List<IdentityError>();

			ValidateNames(user, errors);
			ValidatePersonalNumber(user, errors);
			await ValidateUniquePersonalNumberAsync(manager, user, errors);

			return errors.Count == 0
				? IdentityResult.Success
				: IdentityResult.Failed(errors.ToArray());
		}

		private static async Task ValidateUniquePersonalNumberAsync(
	UserManager<ApplicationUser> manager,
	ApplicationUser user,
	List<IdentityError> errors)
		{
			var personalNumber = user.PersonalNumber?.Trim();

			if (string.IsNullOrWhiteSpace(personalNumber))
				return;

			if (!HasValidPersonalNumberFormat(personalNumber))
				return;

			var exists = await manager.Users.AnyAsync(u =>
				u.PersonalNumber == personalNumber &&
				u.Id != user.Id);

			if (exists) {
				errors.Add(CreateError(
					"PersonalNumberNotUnique",
					"Person number already exists."));
			}
		}

		private static void ValidateNames(ApplicationUser user, List<IdentityError> errors)
		{
			var firstName = user.FirstName?.Trim();
			var lastName = user.LastName?.Trim();

			if (string.IsNullOrWhiteSpace(firstName) ||
				string.IsNullOrWhiteSpace(lastName))
				return;

			if (firstName.Equals(lastName, StringComparison.OrdinalIgnoreCase)) {
				errors.Add(new IdentityError {
					Code = "FirstNameEqualsLastName",
					Description = "First name and last name cannot be the same."
				});
			}
		}

		private static void ValidatePersonalNumber(ApplicationUser user, List<IdentityError> errors)
		{
			var personalNumber = user.PersonalNumber?.Trim();

			if (string.IsNullOrWhiteSpace(personalNumber))
				return;

			if (!HasValidPersonalNumberFormat(personalNumber)) {
				errors.Add(CreateError(
					"PersonalNumberFormat",
					"Personal number must be in the format YYYYMMDD-XXXX."));
				return;
			}

			if (!HasReasonableYear(personalNumber)) {
				errors.Add(CreateError(
					"PersonalNumberYear",
					"Personal number year is not reasonable."));
			}
		}

		private static bool HasValidPersonalNumberFormat(string personalNumber)
		{
			return PersonalNumberRegex.IsMatch(personalNumber);
		}

		private static bool HasReasonableYear(string personalNumber)
		{
			var year = int.Parse(personalNumber.Substring(0, 4));
			var currentYear = DateTime.Now.Year;

			return year >= 1900 && year <= currentYear;
		}

		private static IdentityError CreateError(string code, string description)
		{
			return new IdentityError {
				Code = code,
				Description = description
			};
		}
	}
}