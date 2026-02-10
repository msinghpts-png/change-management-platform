import CalendarMonthOutlinedIcon from "@mui/icons-material/CalendarMonthOutlined";
import DashboardOutlinedIcon from "@mui/icons-material/DashboardOutlined";
import DescriptionOutlinedIcon from "@mui/icons-material/DescriptionOutlined";
import ListAltOutlinedIcon from "@mui/icons-material/ListAltOutlined";
import MenuIcon from "@mui/icons-material/Menu";
import ViewModuleOutlinedIcon from "@mui/icons-material/ViewModuleOutlined";
import {
  AppBar,
  Box,
  Divider,
  Drawer,
  IconButton,
  List,
  ListItemButton,
  ListItemIcon,
  ListItemText,
  Toolbar,
  Typography
} from "@mui/material";
import { useMemo, useState, type ReactNode } from "react";
import { NavLink, useLocation } from "react-router-dom";
import { routes } from "../routes";

type MainLayoutProps = {
  title: string;
  children: ReactNode;
};

const drawerWidth = 240;

const MainLayout = ({ title, children }: MainLayoutProps) => {
  const [mobileOpen, setMobileOpen] = useState(false);
  const location = useLocation();

  const iconMap = useMemo(
    () => ({
      "/dashboard": <DashboardOutlinedIcon />,
      "/changes": <ListAltOutlinedIcon />,
      "/changes/new": <DescriptionOutlinedIcon />,
      "/calendar": <CalendarMonthOutlinedIcon />,
      "/templates": <ViewModuleOutlinedIcon />
    }),
    []
  );

  const drawerContent = (
    <Box>
      <Toolbar>
        <Typography variant="h6">{title}</Typography>
      </Toolbar>
      <Divider />
      <List>
        {routes
          .filter((route) => route.path !== "/changes/:id")
          .map((route) => (
            <ListItemButton
              key={route.path}
              component={NavLink}
              to={route.path}
              selected={location.pathname === route.path || (route.path === "/changes" && location.pathname.startsWith("/changes/"))}
              onClick={() => setMobileOpen(false)}
            >
              <ListItemIcon>{iconMap[route.path as keyof typeof iconMap] ?? <DashboardOutlinedIcon />}</ListItemIcon>
              <ListItemText primary={route.label} />
            </ListItemButton>
          ))}
      </List>
    </Box>
  );

  return (
    <Box sx={{ display: "flex" }}>
      <AppBar position="fixed" sx={{ zIndex: (theme) => theme.zIndex.drawer + 1 }}>
        <Toolbar>
          <IconButton color="inherit" edge="start" onClick={() => setMobileOpen(!mobileOpen)} sx={{ mr: 2, display: { md: "none" } }}>
            <MenuIcon />
          </IconButton>
          <Typography variant="h6" noWrap>
            {title}
          </Typography>
        </Toolbar>
      </AppBar>

      <Box component="nav" sx={{ width: { md: drawerWidth }, flexShrink: { md: 0 } }}>
        <Drawer
          variant="temporary"
          open={mobileOpen}
          onClose={() => setMobileOpen(false)}
          ModalProps={{ keepMounted: true }}
          sx={{
            display: { xs: "block", md: "none" },
            "& .MuiDrawer-paper": { boxSizing: "border-box", width: drawerWidth }
          }}
        >
          {drawerContent}
        </Drawer>
        <Drawer
          variant="permanent"
          sx={{
            display: { xs: "none", md: "block" },
            "& .MuiDrawer-paper": { boxSizing: "border-box", width: drawerWidth }
          }}
          open
        >
          {drawerContent}
        </Drawer>
      </Box>

      <Box component="main" sx={{ flexGrow: 1, p: 3 }}>
        <Toolbar />
        {children}
      </Box>
    </Box>
  );
};

export default MainLayout;
