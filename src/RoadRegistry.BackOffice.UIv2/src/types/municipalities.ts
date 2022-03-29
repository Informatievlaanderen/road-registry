export namespace Municipalities {
    export interface GetMunicipalitiesAPIResponse {
        gemeenten: Gemeenten[];
        volgende: string;
    }

    export interface Gemeenten {
        identificator: Identificator;
        detail: string;
        gemeentenaam: Gemeentenaam;
        gemeenteStatus: GemeenteStatus;
    }

    export enum GemeenteStatus {
        Gehistoreerd = "gehistoreerd",
        InGebruik = "inGebruik",
    }

    export interface Gemeentenaam {
        geografischeNaam: GeografischeNaam;
    }

    export interface GeografischeNaam {
        spelling: string;
        taal: Taal;
    }

    export enum Taal {
        Nl = "nl",
        Fr = "fr",
    }

    export interface Identificator {
        id: string;
        naamruimte: string;
        objectId: string;
        versieId: string;
    }
}
export default Municipalities;