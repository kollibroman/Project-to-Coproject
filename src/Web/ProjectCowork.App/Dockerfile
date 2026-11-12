FROM node:24-slim AS build
WORKDIR /src/Web/ProjectCowork.App

COPY ["Web/ProjectCowork.App/package.json", "Web/ProjectCowork.App/package-lock.json", "./"]
RUN npm ci

COPY ["Web/ProjectCowork.Api/project-cowork.json", "/src/Web/ProjectCowork.Api/project-cowork.json"]
COPY ["Web/ProjectCowork.App/", "./"]

ARG VITE_API_URL=http://localhost:8080
ENV VITE_API_URL=$VITE_API_URL

RUN npm run build

FROM nginx:1.29-alpine AS final
COPY ["Web/ProjectCowork.App/nginx.conf", "/etc/nginx/conf.d/default.conf"]
COPY --from=build /src/Web/ProjectCowork.App/dist /usr/share/nginx/html
EXPOSE 80
