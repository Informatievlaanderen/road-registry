import RoadRegistry from "@/types/road-registry";

const ValidationUtils = {
    convertValidationErrorsToFileProblems: (validationErrors: any) => {
        return Object.keys(validationErrors).map((key: string) => {
            return {
              file: key,
              problems: validationErrors[key].map((validationError: any) => ({
                severity: "Error",
                text: validationError.reason,
              })),
            };
          });
    },

    convertValidationErrorsToArray: (validationErrors: any): RoadRegistry.ValidationError[] => {
        return Object.keys(validationErrors).flatMap((key: string) => {
            return validationErrors[key].map((validationError: any) => ({
                code: key,
                reason: validationError.reason,
              }))
          });
    }
    
}

export default ValidationUtils;