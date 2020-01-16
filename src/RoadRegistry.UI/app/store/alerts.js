import alertTypes from './alert-types';

export default {
  generalError: {
    title: 'Er is een fout opgetreden',
    content: 'Er is een algemene fout opgetreden.',
    type: alertTypes.error,
  },
  connectionError: {
    title: 'Er is een fout opgetreden',
    content: 'De server kon niet worden bereikt.',
    type: alertTypes.error,
  },
  importWarning: {
    title: 'Opgelet!',
    content: 'Het wegenregister wordt momenteel geimporteerd. Downloaden en uploaden zijn nu niet mogelijk. Probeer het later opnieuw.',
    type: alertTypes.warning,
  },
  createDomainError(detail) {
    return {
      title: 'Er is een fout opgetreden',
      content: detail,
      type: alertTypes.error,
    };
  },
  serverError: {
    title: 'Er is een fout opgetreden',
    content: 'De server kan dit verzoek niet correct behandelen.',
    type: alertTypes.error,
  },
  empty: {
    title: '',
    content: '',
    type: alertTypes.error,
    visible: false,
  },
  toAlert(error) {
    if (error.response) {
      if (error.response.status === 400 &&
        error.response.data &&
        error.response.data.detail) {
        return this.createDomainError(error.response.data.detail);
      }
      if (error.response.status === 503 &&
        error.request.responseURL.endsWith('/v1/download')) {
        return this.importWarning;
      }
      return this.serverError;
    } else if (error.request) {
      return this.connectionError;
    }
    return this.generalError;
  },
};
