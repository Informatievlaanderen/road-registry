FROM nginx:stable-alpine as production-stage

LABEL maintainer "agentschap Informatie Vlaanderen <informatie.vlaanderen@vlaanderen.be>"

COPY default.conf /etc/nginx/conf.d/default.conf
COPY /wwwroot /usr/share/nginx/html
COPY init.sh ./init.sh
COPY .htpasswd /etc/nginx/.htpasswd

RUN chmod +x ./init.sh

EXPOSE 10007

ENTRYPOINT ["./init.sh"]
