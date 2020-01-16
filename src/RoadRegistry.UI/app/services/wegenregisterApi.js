import axios from 'axios';
import fileSaver from 'file-saver';

export default {
  downloadCompleteRegistry(configuration) {
    const { onDownloadProgress = () => {} } = configuration;

    return axios
      .get(
        '/v1/download',
        {
          transformRequest: [(data, headers) => {
            // eslint-disable-next-line
            delete headers.common.Accept;
            return data;
          }],
          onDownloadProgress,
          headers: {
            // 'Access-Control-Expose-Headers': 'Content-Disposition',
            'Content-Type': 'application/zip',
          },
          responseType: 'blob',
        },
      )
      .then((response) => {
        // if I get the CORS settings correct, the filename can be retrieved from response.Headers['Content-Disposition']
        fileSaver.saveAs(response.data, 'wegenregister.zip');
      });
  },
};
