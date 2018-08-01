import alertTypes from './alert-types';

export default {
  dienstverleningAangemaakt: {
    title: 'Dienstverlening aangemaakt',
    content: 'De dienstverlening werd aangemaakt.', // TODO: url
    type: alertTypes.success,
  },
  dienstverleningAangepast: {
    title: 'Dienstverlening aangepast',
    content: 'De aanpassing werd opgeslagen.', // TODO: url
    type: alertTypes.success,
  },
};
