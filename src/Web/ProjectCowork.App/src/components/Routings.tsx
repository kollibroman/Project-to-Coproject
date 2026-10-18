import {createBrowserRouter, createRoutesFromElements, Route} from "react-router";
import {GuestNoAuthRedirect} from "../Utils/GuestNoAuthRedirect.tsx";
import {RequireAuthGuard} from "../Utils/RequireAuthGuard.tsx";
import {AuthPage} from "../Pages/AuthPage.tsx";
import {AppLayout} from "../Layout/AppLayout.tsx";
import {ProjectsPage} from "../Pages/ProjectsPage.tsx";
import {SettingsPage} from "../Pages/SettingsPage.tsx";
import Home from "../App.tsx";

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
                    <Route path="/settings" element={<SettingsPage />}/>
                </Route>
            </Route>
        </Route>,
    ),
);
