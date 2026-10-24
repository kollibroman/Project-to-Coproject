import {createBrowserRouter, createRoutesFromElements, Route} from "react-router";
import {GuestNoAuthRedirect} from "../Utils/GuestNoAuthRedirect.tsx";
import {RequireAuthGuard} from "../Utils/RequireAuthGuard.tsx";
import {AuthPage} from "../Features/Auth/Pages/AuthPage.tsx";
import {AppLayout} from "../Layout/AppLayout.tsx";
import {ProjectsPage} from "../Features/Projects/Pages/ProjectsPage.tsx";
import {SettingsPage} from "../Features/Settings/Pages/SettingsPage.tsx";
import Home from "../App.tsx";
import {ProjectPage} from "../Features/Projects/Pages/ProjectPage.tsx";
import {DiscoverPage} from "../Features/Projects/Pages/DiscoverPage.tsx";
import {PostingsPage} from "../Features/Postings/Pages/PostingsPage.tsx";
import {RepositoryPage} from "../Features/Repositories/Pages/RepositoryPage.tsx";

export const Routings = createBrowserRouter(
    createRoutesFromElements(
        <Route>
            <Route element={<GuestNoAuthRedirect />}>
                <Route path="/auth" element={<AuthPage/>}/>
            </Route>
            <Route element={<RequireAuthGuard />}>
                <Route element={<AppLayout />}>
                    <Route path="/" element={<Home />}/>
                    <Route path="/projects" element={<ProjectsPage />}/>
                    <Route path="/discover" element={<DiscoverPage />}/>
                    <Route path="/postings" element={<PostingsPage />}/>
                    <Route path={"/project/:projectId"} element={<ProjectPage />}/>
                    <Route path={"/repository/:repositoryId"} element={<RepositoryPage />}/>
                    <Route path="/settings" element={<SettingsPage />}/>
                </Route>
            </Route>
        </Route>,
    ),
);
