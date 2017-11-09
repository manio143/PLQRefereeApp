FROM mono

LABEL maintainer=marian.dziubiak@gmail.com

ENV path /var/app

ADD build $path
ADD css $path/css
ADD js $path/js
ADD templates $path/templates

ENV port 80
EXPOSE 80

WORKDIR ${path}
CMD ["mono", "/var/app/PLQRefereeApp.exe"]
