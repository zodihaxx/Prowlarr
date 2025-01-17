using System.Linq;
using System.Text.RegularExpressions;
using FluentValidation;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Annotations;
using NzbDrone.Core.Validation;

namespace NzbDrone.Core.Indexers.Newznab
{
    public class NewznabSettingsValidator : AbstractValidator<NewznabSettings>
    {
        private static readonly string[] ApiKeyWhiteList =
        {
            "nzbs.org",
            "nzb.su",
            "dognzb.cr",
            "nzbplanet.net",
            "nzbid.org",
            "nzbndx.com",
            "nzbindex.in"
        };

        private static bool ShouldHaveApiKey(NewznabSettings settings)
        {
            return settings.BaseUrl != null && ApiKeyWhiteList.Any(c => settings.BaseUrl.ToLowerInvariant().Contains(c));
        }

        private static readonly Regex AdditionalParametersRegex = new Regex(@"(&.+?\=.+?)+", RegexOptions.Compiled);

        public NewznabSettingsValidator()
        {
            RuleFor(x => x.BaseSettings).SetValidator(new IndexerCommonSettingsValidator());
            RuleFor(c => c.BaseUrl).ValidRootUrl();
            RuleFor(c => c.ApiPath).ValidUrlBase("/api");
            RuleFor(c => c.ApiKey).NotEmpty().When(ShouldHaveApiKey);
            RuleFor(c => c.AdditionalParameters).Matches(AdditionalParametersRegex)
                                                .When(c => !c.AdditionalParameters.IsNullOrWhiteSpace());

            RuleFor(c => c.VipExpiration).Must(c => c.IsValidDate())
                                         .When(c => c.VipExpiration.IsNotNullOrWhiteSpace())
                                         .WithMessage("Correctly formatted date is required");

            RuleFor(c => c.VipExpiration).Must(c => c.IsFutureDate())
                                         .When(c => c.VipExpiration.IsNotNullOrWhiteSpace())
                                         .WithMessage("Must be a future date")
                                         .AsWarning();
        }
    }

    public class NewznabSettings : IIndexerSettings
    {
        private static readonly NewznabSettingsValidator Validator = new ();

        public NewznabSettings()
        {
            ApiPath = "/api";
            VipExpiration = "";
        }

        [FieldDefinition(0, Label = "URL")]
        public string BaseUrl { get; set; }

        [FieldDefinition(1, Label = "IndexerSettingsApiPath", HelpText = "IndexerSettingsApiPathHelpText", Advanced = true)]
        [FieldToken(TokenField.HelpText, "IndexerSettingsApiPath", "url", "/api")]
        public string ApiPath { get; set; }

        [FieldDefinition(2, Label = "ApiKey", HelpText = "IndexerNewznabSettingsApiKeyHelpText", Privacy = PrivacyLevel.ApiKey)]
        public string ApiKey { get; set; }

        [FieldDefinition(5, Label = "IndexerSettingsAdditionalParameters", HelpText = "IndexerNewznabSettingsAdditionalParametersHelpText", Advanced = true)]
        public string AdditionalParameters { get; set; }

        [FieldDefinition(6, Label = "IndexerSettingsVipExpiration", HelpText = "IndexerNewznabSettingsVipExpirationHelpText")]
        public string VipExpiration { get; set; }

        [FieldDefinition(7)]
        public IndexerBaseSettings BaseSettings { get; set; } = new ();

        public NewznabCapabilitiesSettings Capabilities { get; set; }

        // Field 8 is used by TorznabSettings MinimumSeeders
        // If you need to add another field here, update TorznabSettings as well and this comment
        public virtual NzbDroneValidationResult Validate()
        {
            return new NzbDroneValidationResult(Validator.Validate(this));
        }
    }
}
