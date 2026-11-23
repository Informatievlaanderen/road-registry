namespace RoadRegistry.BackOffice.Api.IntegrationTests.Authentication
{
    using System.Net.Http;
    using Be.Vlaanderen.Basisregisters.Auth.AcmIdm;

    // Every endpoint of the BackOffice API, with the scopes it requires. AcmIdmTests calls each of them with and without
    // those scopes; RoadRegistry.BackOffice.Api.Tests compiles this file too and checks, on every build, that the list
    // is complete - so a new endpoint fails the build until it is added here.
    public static class AcmIdmEndpoints
    {
        public sealed record Endpoint(HttpMethod Method, string Uri, params string[] RequiredScopes);

        public static readonly Endpoint[] All =
        {
            new(HttpMethod.Get, "v1/changefeed/entry/{id}/content"),
            new(HttpMethod.Get, "v1/changefeed/head"),
            new(HttpMethod.Get, "v1/changefeed/next"),
            new(HttpMethod.Get, "v1/changefeed/previous"),

            new(HttpMethod.Post, "v1/extracten/{downloadId}/sluit", Scopes.DvWrIngemetenWegBeheer),
            new(HttpMethod.Get, "v1/extracten/{downloadId}/download", Scopes.DvWrIngemetenWegBeheer),
            new(HttpMethod.Get, "v1/extracten/{downloadId}/upload", Scopes.DvWrIngemetenWegBeheer),
            new(HttpMethod.Post, "v1/extracten/{downloadId}/upload", Scopes.DvWrIngemetenWegBeheer),
            new(HttpMethod.Get, "v1/extracten/{downloadId}", Scopes.DvWrIngemetenWegBeheer),
            new(HttpMethod.Get, "v1/extracten", Scopes.DvWrIngemetenWegBeheer),
            new(HttpMethod.Post, "v1/extracten/downloadaanvragen/percontour", Scopes.DvWrIngemetenWegBeheer),
            new(HttpMethod.Post, "v1/extracten/downloadaanvragen/perbestand", Scopes.DvWrIngemetenWegBeheer),
            new(HttpMethod.Post, "v1/extracten/downloadaanvragen/perniscode", Scopes.DvWrIngemetenWegBeheer),
            new(HttpMethod.Post, "v1/extracten/overlapping/perniscode", Scopes.DvWrIngemetenWegBeheer),
            new(HttpMethod.Post, "v1/extracten/overlapping/percontour", Scopes.DvWrIngemetenWegBeheer),

            new(HttpMethod.Post, "v2/extracten/{downloadId}/sluit", Scopes.DvWrIngemetenWegBeheer),
            new(HttpMethod.Get, "v2/extracten/{downloadId}/download", Scopes.DvWrIngemetenWegBeheer),
            new(HttpMethod.Get, "v2/extracten/{downloadId}/upload", Scopes.DvWrIngemetenWegBeheer),
            new(HttpMethod.Post, "v2/extracten/{downloadId}/upload", Scopes.DvWrIngemetenWegBeheer),
            new(HttpMethod.Get, "v2/extracten/{downloadId}", Scopes.DvWrIngemetenWegBeheer),
            new(HttpMethod.Get, "v2/extracten", Scopes.DvWrIngemetenWegBeheer),
            new(HttpMethod.Post, "v2/extracten/downloadaanvragen/percontour", Scopes.DvWrIngemetenWegBeheer),
            new(HttpMethod.Post, "v2/extracten/downloadaanvragen/perbestand", Scopes.DvWrIngemetenWegBeheer),
            new(HttpMethod.Post, "v2/extracten/downloadaanvragen/perniscode", Scopes.DvWrIngemetenWegBeheer),
            new(HttpMethod.Post, "v2/extracten/overlapping/perniscode", Scopes.DvWrIngemetenWegBeheer),
            new(HttpMethod.Post, "v2/extracten/overlapping/percontour", Scopes.DvWrIngemetenWegBeheer),

            new(HttpMethod.Get, "v1/information"),
            new(HttpMethod.Post, "v1/information/validate-wkt", Scopes.DvWrIngemetenWegBeheer),

            new(HttpMethod.Get, "v1/security/user", Scopes.VoInfo),
            new(HttpMethod.Get, "v1/security/info"),
            new(HttpMethod.Get, "v1/security/exchange"),

            new(HttpMethod.Post, "v1/system/healthcheck", Scopes.DvWrUitzonderingenBeheer),
            new(HttpMethod.Post, "v1/system/snapshots/refresh", Scopes.DvWrUitzonderingenBeheer),

            new(HttpMethod.Get, "v1/organizations"),
            new(HttpMethod.Post, "v1/organizations", Scopes.DvWrUitzonderingenBeheer),
            new(HttpMethod.Patch, "v1/organizations/{id}", Scopes.DvWrUitzonderingenBeheer),
            new(HttpMethod.Delete, "v1/organizations/{id}", Scopes.DvWrUitzonderingenBeheer),
            new(HttpMethod.Patch, "v1/organizations/{id}/rename", Scopes.DvWrUitzonderingenBeheer),

            new(HttpMethod.Post, "v1/wegsegmenten/acties/wijzigen/attributen", Scopes.DvWrAttribuutWaardenBeheer),
            new(HttpMethod.Post, "v1/wegsegmenten/acties/wijzigen/dynamischeattributen", Scopes.DvWrAttribuutWaardenBeheer),
            new(HttpMethod.Post, "v1/wegsegmenten/acties/schetsen", Scopes.DvWrGeschetsteWegBeheer),
            new(HttpMethod.Post, "v1/wegsegmenten/acties/verwijderen", Scopes.DvWrUitzonderingenBeheer),
            new(HttpMethod.Get, "v1/wegsegmenten/{id}"),
            new(HttpMethod.Post, "v1/wegsegmenten/{id}/acties/straatnaamkoppelen", Scopes.DvWrAttribuutWaardenBeheer),
            new(HttpMethod.Post, "v1/wegsegmenten/{id}/acties/straatnaamontkoppelen", Scopes.DvWrAttribuutWaardenBeheer),
            new(HttpMethod.Post, "v1/wegsegmenten/{id}/acties/verwijderen/schets", Scopes.DvWrGeschetsteWegBeheer),
            new(HttpMethod.Post, "v1/wegsegmenten/{id}/acties/wijzigen/schetsgeometrie", Scopes.DvWrGeschetsteWegBeheer),

            new(HttpMethod.Post, "v1/inwinning/downloadaanvraag", Scopes.DvWrIngemetenWegBeheer),
            new(HttpMethod.Get, "v1/inwinning/extracten", Scopes.DvWrIngemetenWegBeheer),
            new(HttpMethod.Get, "v1/inwinning/niscodes", Scopes.DvWrIngemetenWegBeheer),
            new(HttpMethod.Post, "v1/inwinning/{downloadId}/upload", Scopes.DvWrIngemetenWegBeheer),
            new(HttpMethod.Get, "v1/inwinningsstatus/gemeente/{nisCode}"),
            new(HttpMethod.Get, "v1/inwinningsstatus/wegsegment/{id}"),

            new(HttpMethod.Get, "v2/wegknopen/{id}"),
            new(HttpMethod.Post, "v2/wegsegmenten/acties/schetsen", Scopes.DvWrGeschetsteWegBeheer),
            new(HttpMethod.Get, "v2/wegsegmenten"),
            new(HttpMethod.Get, "v2/wegsegmenten/{id}"),
            new(HttpMethod.Post, "v2/wegsegmenten/{id}/acties/knippen", Scopes.DvWrGeschetsteWegBeheer),
            new(HttpMethod.Post, "v2/wegsegmenten/{id}/acties/wijzigen/geometrie", Scopes.DvWrGeschetsteWegBeheer, Scopes.DvWrIngemetenWegBeheer),
            new(HttpMethod.Post, "v2/wegsegmenten/{id}/acties/geplandnaargerealiseerd", Scopes.DvWrGeschetsteWegBeheer, Scopes.DvWrIngemetenWegBeheer),
            new(HttpMethod.Post, "v2/wegsegmenten/{id}/acties/geplandnaarnietgerealiseerd", Scopes.DvWrGeschetsteWegBeheer, Scopes.DvWrIngemetenWegBeheer),
            new(HttpMethod.Post, "v2/wegsegmenten/{id}/acties/buitengebruiknaargerealiseerd", Scopes.DvWrGeschetsteWegBeheer, Scopes.DvWrIngemetenWegBeheer),
            new(HttpMethod.Post, "v2/wegsegmenten/{id}/acties/gerealiseerdnaarbuitengebruik", Scopes.DvWrGeschetsteWegBeheer, Scopes.DvWrIngemetenWegBeheer),
            new(HttpMethod.Post, "v2/wegsegmenten/{id}/acties/gerealiseerdnaargehistoreerd", Scopes.DvWrGeschetsteWegBeheer, Scopes.DvWrIngemetenWegBeheer),
            new(HttpMethod.Post, "v2/wegsegmenten/{id}/acties/buitengebruiknaargehistoreerd", Scopes.DvWrGeschetsteWegBeheer, Scopes.DvWrIngemetenWegBeheer),
            new(HttpMethod.Post, "v2/wegsegmenten/{id}/acties/corrigeren/gerealiseerdnaargepland", Scopes.DvWrGeschetsteWegBeheer, Scopes.DvWrIngemetenWegBeheer),
            new(HttpMethod.Post, "v2/wegsegmenten/{id}/acties/corrigeren/nietgerealiseerdnaargepland", Scopes.DvWrGeschetsteWegBeheer, Scopes.DvWrIngemetenWegBeheer),
            new(HttpMethod.Post, "v2/wegsegmenten/{id}/acties/corrigeren/gehistoreerdnaargerealiseerd", Scopes.DvWrGeschetsteWegBeheer, Scopes.DvWrIngemetenWegBeheer),
            new(HttpMethod.Post, "v2/wegsegmenten/{id}/acties/corrigeren/gehistoreerdnaarbuitengebruik", Scopes.DvWrGeschetsteWegBeheer, Scopes.DvWrIngemetenWegBeheer),
            new(HttpMethod.Post, "v2/wegsegmenten/acties/knippenopkruising", Scopes.DvWrGeschetsteWegBeheer),
            new(HttpMethod.Post, "v2/wegsegmenten/acties/wijzigen/attributen", Scopes.DvWrAttribuutWaardenBeheer),
            new(HttpMethod.Post, "v2/wegknopen/acties/wijzigen/attributen", Scopes.DvWrAttribuutWaardenBeheer),
            new(HttpMethod.Post, "v2/wegknopen/{id}/acties/verwijderen", Scopes.DvWrGeschetsteWegBeheer),
            new(HttpMethod.Post, "v2/wegsegmenten/acties/verwijderen", Scopes.DvWrUitzonderingenBeheer),
            new(HttpMethod.Post, "v2/gelijkgrondsekruisingen/{id}/acties/wijzigen/naarongelijkgrondsekruising", Scopes.DvWrGeschetsteWegBeheer),
            new(HttpMethod.Post, "v2/ongelijkgrondsekruisingen/{id}/acties/wijzigen/naargelijkgrondsekruising", Scopes.DvWrGeschetsteWegBeheer),
            new(HttpMethod.Post, "v2/ongelijkgrondsekruisingen/{id}/acties/wijzigen/attributen", Scopes.DvWrAttribuutWaardenBeheer),
            new(HttpMethod.Post, "v2/wegsegmenten/acties/wijzigen/geometriemethode", Scopes.DvWrIngemetenWegBeheer),
            new(HttpMethod.Get, "v2/gelijkgrondsekruisingen/{id}"),
            new(HttpMethod.Get, "v2/ongelijkgrondsekruisingen/{id}"),
        };
    }
}
