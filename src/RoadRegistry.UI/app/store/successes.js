import alertTypes from './alert-types';

export default {
  downloadRegistryStarted: {
    title: 'Voorbereiding download wegenregister gestart',
    content: 'Wegenregister download zal gestart worden zodra de bestanden samengesteld zijn.',
    type: alertTypes.success,
  },
  downloadRegistryCompleted: {
    title: 'Download wegenregister gestart',
    content: 'Het wegenregister wordt gedownload naar uw toestel.',
    type: alertTypes.success,
  },
};
